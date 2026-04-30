using System.Security.Cryptography;
using System.Text;
using FlashMediator;
using FluentValidation;
using Grpc.Core;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Deduct;
using Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;
using Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze;
using Licit.WalletService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.API.Grpc;

public class WalletInternalGrpcService(
    IMediator mediator,
    IConfiguration configuration) : WalletInternal.WalletInternalBase
{
    private const string ServiceKeyHeader = "x-licit-service-key";

    public override async Task<WalletOperationResponse> Freeze(
        WalletOperationRequest request,
        ServerCallContext context)
    {
        EnsureAuthorized(context);

        try
        {
            var userId = ParseGuid(request.UserId, "user_id");
            var referenceId = ParseGuid(request.ReferenceId, "reference_id");
            var amount = ParseAmount(request.AmountCents);

            var result = await mediator.Send(new FreezeFundsCommandRequest(
                userId,
                amount,
                referenceId,
                NormalizeDescription(request.Description)));

            return ToGrpcResponse(
                result.TransactionId,
                result.AvailableBalance,
                result.FrozenBalance,
                result.CreatedAt,
                result.IdempotentReplay);
        }
        catch (Exception exception) when (exception is not RpcException)
        {
            throw ToRpcException(exception);
        }
    }

    public override async Task<WalletOperationResponse> Unfreeze(
        WalletOperationRequest request,
        ServerCallContext context)
    {
        EnsureAuthorized(context);

        try
        {
            var userId = ParseGuid(request.UserId, "user_id");
            var referenceId = ParseGuid(request.ReferenceId, "reference_id");
            var amount = ParseAmount(request.AmountCents);

            var result = await mediator.Send(new UnfreezeFundsCommandRequest(
                userId,
                amount,
                referenceId,
                NormalizeDescription(request.Description)));

            return ToGrpcResponse(
                result.TransactionId,
                result.AvailableBalance,
                result.FrozenBalance,
                result.CreatedAt,
                result.IdempotentReplay);
        }
        catch (Exception exception) when (exception is not RpcException)
        {
            throw ToRpcException(exception);
        }
    }

    public override async Task<WalletOperationResponse> Deduct(
        WalletOperationRequest request,
        ServerCallContext context)
    {
        EnsureAuthorized(context);

        try
        {
            var userId = ParseGuid(request.UserId, "user_id");
            var referenceId = ParseGuid(request.ReferenceId, "reference_id");
            var amount = ParseAmount(request.AmountCents);

            var result = await mediator.Send(new DeductFundsCommandRequest(
                userId,
                amount,
                referenceId,
                NormalizeDescription(request.Description)));

            return ToGrpcResponse(
                result.TransactionId,
                result.AvailableBalance,
                result.FrozenBalance,
                result.CreatedAt,
                result.IdempotentReplay);
        }
        catch (Exception exception) when (exception is not RpcException)
        {
            throw ToRpcException(exception);
        }
    }

    private void EnsureAuthorized(ServerCallContext context)
    {
        var expectedKey = configuration["InternalGrpc:ServiceKey"];
        var providedKey = context.RequestHeaders
            .FirstOrDefault(entry => string.Equals(entry.Key, ServiceKeyHeader, StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (!IsValidServiceKey(expectedKey, providedKey))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing or invalid service key."));
    }

    private static bool IsValidServiceKey(string? expectedKey, string? providedKey)
    {
        if (string.IsNullOrWhiteSpace(expectedKey) || string.IsNullOrEmpty(providedKey))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedKey);

        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private static Guid ParseGuid(string value, string fieldName)
    {
        if (Guid.TryParse(value, out var parsed) && parsed != Guid.Empty)
            return parsed;

        throw new RpcException(new Status(StatusCode.InvalidArgument, $"{fieldName} must be a valid GUID."));
    }

    private static decimal ParseAmount(long amountCents)
    {
        if (amountCents <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "amount_cents must be greater than zero."));

        return amountCents / 100m;
    }

    private static string? NormalizeDescription(string description) =>
        string.IsNullOrWhiteSpace(description) ? null : description;

    private static WalletOperationResponse ToGrpcResponse(
        Guid transactionId,
        decimal availableBalance,
        decimal frozenBalance,
        DateTime createdAt,
        bool idempotentReplay) =>
        new()
        {
            TransactionId = transactionId.ToString(),
            AvailableBalanceCents = ToCents(availableBalance),
            FrozenBalanceCents = ToCents(frozenBalance),
            CreatedAt = createdAt.ToUniversalTime().ToString("O"),
            IdempotentReplay = idempotentReplay
        };

    private static long ToCents(decimal amount) =>
        checked((long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));

    private static RpcException ToRpcException(Exception exception) =>
        exception switch
        {
            ValidationException validationException => new RpcException(new Status(
                StatusCode.InvalidArgument,
                string.Join("; ", validationException.Errors.Select(error => error.ErrorMessage)))),
            NotFoundException => new RpcException(new Status(StatusCode.NotFound, exception.Message)),
            ConcurrencyException or DbUpdateConcurrencyException => new RpcException(new Status(StatusCode.Aborted, exception.Message)),
            InsufficientBalanceException or InsufficientFrozenBalanceException => new RpcException(new Status(StatusCode.FailedPrecondition, exception.Message)),
            InvalidAmountException => new RpcException(new Status(StatusCode.InvalidArgument, exception.Message)),
            ConflictException => new RpcException(new Status(StatusCode.FailedPrecondition, exception.Message)),
            BaseException => new RpcException(new Status(StatusCode.Unknown, exception.Message)),
            _ => new RpcException(new Status(StatusCode.Unknown, "Wallet operation failed."))
        };
}

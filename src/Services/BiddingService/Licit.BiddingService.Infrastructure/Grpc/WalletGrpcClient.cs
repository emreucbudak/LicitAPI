using Grpc.Core;
using Licit.BiddingService.Application.DTOs;
using Licit.BiddingService.Application.Interfaces;
using Licit.WalletService.API.Grpc;
using Microsoft.Extensions.Options;

namespace Licit.BiddingService.Infrastructure.Grpc
{
    public class WalletGrpcClient(
        WalletInternal.WalletInternalClient client,
        IOptions<WalletGrpcOptions> options) : IWalletClient
    {
        private const string ServiceKeyHeader = "x-licit-service-key";

        public async Task<WalletHoldResult> TryHoldBalanceAsync(
            Guid userId,
            Guid bidId,
            int amount,
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await client.FreezeAsync(
                    CreateRequest(userId, bidId, amount, idempotencyKey, "Bid amount frozen"),
                    CreateHeaders(),
                    CreateDeadline(),
                    cancellationToken);

                return WalletHoldResult.Held(
                    Guid.Parse(response.TransactionId),
                    response.AvailableBalanceCents,
                    response.FrozenBalanceCents,
                    response.IdempotentReplay);
            }
            catch (RpcException exception) when (exception.StatusCode == StatusCode.FailedPrecondition)
            {
                return WalletHoldResult.Rejected("INSUFFICIENT_BALANCE");
            }
            catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
            {
                return WalletHoldResult.Rejected("WALLET_NOT_FOUND");
            }
            catch (RpcException exception) when (exception.StatusCode == StatusCode.Unauthenticated)
            {
                return WalletHoldResult.Rejected("WALLET_GRPC_UNAUTHORIZED");
            }
        }

        public async Task ReleaseHoldAsync(
            Guid userId,
            Guid bidId,
            int amount,
            CancellationToken cancellationToken)
        {
            await client.UnfreezeAsync(
                CreateRequest(userId, bidId, amount, bidId.ToString("D"), "Bid rejected, frozen amount released"),
                CreateHeaders(),
                CreateDeadline(),
                cancellationToken);
        }

        private WalletOperationRequest CreateRequest(
            Guid userId,
            Guid bidId,
            int amount,
            string operationId,
            string description) =>
            new()
            {
                UserId = userId.ToString("D"),
                AmountCents = ToCents(amount),
                ReferenceId = bidId.ToString("D"),
                OperationId = operationId,
                Description = description
            };

        private Metadata CreateHeaders()
        {
            var headers = new Metadata();
            if (!string.IsNullOrWhiteSpace(options.Value.ServiceKey))
                headers.Add(ServiceKeyHeader, options.Value.ServiceKey);

            return headers;
        }

        private DateTime? CreateDeadline()
        {
            if (options.Value.DeadlineSeconds <= 0)
                return null;

            return DateTime.UtcNow.AddSeconds(options.Value.DeadlineSeconds);
        }

        private static long ToCents(int amount)
            => checked(amount * 100L);
    }
}

using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;

public class DepositFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IWalletRepository walletRepository,
    IDepositIdempotencyStore idempotencyStore,
    IValidator<DepositFundsCommandRequest> validator) : IRequestHandler<DepositFundsCommandRequest, DepositFundsCommandResponse>
{
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromMinutes(2);

    public async Task<DepositFundsCommandResponse> Handle(DepositFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId);

        if (wallet is not null && request.ReferenceId.HasValue)
        {
            var existingTransaction = await walletRepository.GetTransactionByWalletTypeAndReferenceAsync(
                wallet.Id,
                Domain.Entities.TransactionType.Deposit,
                request.ReferenceId.Value);

            if (existingTransaction is not null)
                return new DepositFundsCommandResponse(
                    existingTransaction.Id,
                    existingTransaction.BalanceAfter,
                    existingTransaction.FrozenBalanceAfter,
                    existingTransaction.CreatedAt);
        }

        var reserved = await idempotencyStore.TryReserveAsync(
            request.UserId,
            request.IdempotencyKey,
            IdempotencyTtl,
            cancellationToken);

        if (!reserved)
            throw new DuplicateDepositRequestException();

        try
        {
            if (wallet is null)
            {
                wallet = new Domain.Entities.Wallet(request.UserId);
                walletRepository.Add(wallet);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var transaction = wallet.Deposit(request.Amount, request.ReferenceId, request.Description);

            try
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }
            catch (DbUpdateException) when (request.ReferenceId.HasValue)
            {
                var existingTransaction = await walletRepository.GetTransactionByWalletTypeAndReferenceAsync(
                    wallet.Id,
                    Domain.Entities.TransactionType.Deposit,
                    request.ReferenceId.Value);

                if (existingTransaction is not null)
                    return new DepositFundsCommandResponse(
                        existingTransaction.Id,
                        existingTransaction.BalanceAfter,
                        existingTransaction.FrozenBalanceAfter,
                        existingTransaction.CreatedAt);

                throw;
            }

            return new DepositFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch
        {
            await idempotencyStore.ReleaseAsync(request.UserId, request.IdempotencyKey, cancellationToken);
            throw;
        }
    }
}

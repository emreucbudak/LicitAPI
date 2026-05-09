using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze;

public class UnfreezeFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IWalletRepository walletRepository,
    IValidator<UnfreezeFundsCommandRequest> validator) : IRequestHandler<UnfreezeFundsCommandRequest, UnfreezeFundsCommandResponse>
{
    public async Task<UnfreezeFundsCommandResponse> Handle(UnfreezeFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        await using var unitOfWorkTransaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var wallet = await walletRepository.GetByUserIdForUpdateAsync(request.UserId)
            ?? throw new WalletNotFoundException(request.UserId);

        var existingTransaction = await walletRepository.GetTransactionByWalletTypeAndReferenceAsync(
            wallet.Id,
            TransactionType.Unfreeze,
            request.ReferenceId);

        if (existingTransaction is not null)
        {
            await unitOfWorkTransaction.CommitAsync(cancellationToken);
            return new UnfreezeFundsCommandResponse(
                existingTransaction.Id,
                existingTransaction.BalanceAfter,
                existingTransaction.FrozenBalanceAfter,
                existingTransaction.CreatedAt,
                true);
        }

        try
        {
            var transaction = wallet.Unfreeze(request.Amount, request.ReferenceId, request.Description);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWorkTransaction.CommitAsync(cancellationToken);
            return new UnfreezeFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
        catch (DbUpdateException)
        {
            existingTransaction = await walletRepository.GetTransactionByWalletTypeAndReferenceAsync(
                wallet.Id,
                TransactionType.Unfreeze,
                request.ReferenceId);

            if (existingTransaction is not null)
            {
                await unitOfWorkTransaction.CommitAsync(cancellationToken);
                return new UnfreezeFundsCommandResponse(
                    existingTransaction.Id,
                    existingTransaction.BalanceAfter,
                    existingTransaction.FrozenBalanceAfter,
                    existingTransaction.CreatedAt,
                    true);
            }

            throw;
        }
    }
}

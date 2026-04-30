using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;

public class DeductFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DeductFundsCommandRequest> validator) : IRequestHandler<DeductFundsCommandRequest, DeductFundsCommandResponse>
{
    public async Task<DeductFundsCommandResponse> Handle(DeductFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await unitOfWork.Wallets.GetByUserIdAsync(request.UserId)
            ?? throw new WalletNotFoundException(request.UserId);

        var existingTransaction = await unitOfWork.Wallets.GetTransactionByWalletTypeAndReferenceAsync(
            wallet.Id,
            TransactionType.Deduct,
            request.ReferenceId);

        if (existingTransaction is not null)
            return new DeductFundsCommandResponse(
                existingTransaction.Id,
                existingTransaction.BalanceAfter,
                existingTransaction.FrozenBalanceAfter,
                existingTransaction.CreatedAt,
                true);

        try
        {
            var transaction = wallet.Deduct(request.Amount, request.ReferenceId, request.Description);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new DeductFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
        catch (DbUpdateException)
        {
            existingTransaction = await unitOfWork.Wallets.GetTransactionByWalletTypeAndReferenceAsync(
                wallet.Id,
                TransactionType.Deduct,
                request.ReferenceId);

            if (existingTransaction is not null)
                return new DeductFundsCommandResponse(
                    existingTransaction.Id,
                    existingTransaction.BalanceAfter,
                    existingTransaction.FrozenBalanceAfter,
                    existingTransaction.CreatedAt,
                    true);

            throw;
        }
    }
}

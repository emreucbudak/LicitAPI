using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze;

public class FreezeFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IWalletRepository walletRepository,
    IValidator<FreezeFundsCommandRequest> validator) : IRequestHandler<FreezeFundsCommandRequest, FreezeFundsCommandResponse>
{
    public async Task<FreezeFundsCommandResponse> Handle(FreezeFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        await using var unitOfWorkTransaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new WalletNotFoundException(request.UserId);

        var existingTransaction = await walletRepository.GetTransactionByWalletTypeAndReferenceAsync(
            wallet.Id,
            TransactionType.Freeze,
            request.ReferenceId);

        if (existingTransaction is not null)
        {
            await unitOfWorkTransaction.CommitAsync(cancellationToken);
            return new FreezeFundsCommandResponse(
                existingTransaction.Id,
                existingTransaction.BalanceAfter,
                existingTransaction.FrozenBalanceAfter,
                existingTransaction.CreatedAt,
                true);
        }

        try
        {
            var transaction = wallet.Freeze(checked((int)request.Amount), request.ReferenceId, request.Description);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWorkTransaction.CommitAsync(cancellationToken);
            return new FreezeFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
        catch (DbUpdateException)
        {
            existingTransaction = await walletRepository.GetTransactionByWalletTypeAndReferenceAsync(
                wallet.Id,
                TransactionType.Freeze,
                request.ReferenceId);

            if (existingTransaction is not null)
            {
                await unitOfWorkTransaction.CommitAsync(cancellationToken);
                return new FreezeFundsCommandResponse(
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

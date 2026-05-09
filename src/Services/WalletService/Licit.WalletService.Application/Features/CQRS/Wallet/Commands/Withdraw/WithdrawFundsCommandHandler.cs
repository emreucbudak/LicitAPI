using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw;

public class WithdrawFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IWalletRepository walletRepository,
    IValidator<WithdrawFundsCommandRequest> validator) : IRequestHandler<WithdrawFundsCommandRequest, WithdrawFundsCommandResponse>
{
    public async Task<WithdrawFundsCommandResponse> Handle(WithdrawFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        await using var unitOfWorkTransaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var wallet = await walletRepository.GetByUserIdForUpdateAsync(request.UserId)
            ?? throw new WalletNotFoundException(request.UserId);

        try
        {
            var transaction = wallet.Withdraw(request.Amount);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWorkTransaction.CommitAsync(cancellationToken);
            return new WithdrawFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
    }
}

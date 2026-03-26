using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw;

public class WithdrawFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<WithdrawFundsCommandRequest> validator) : IRequestHandler<WithdrawFundsCommandRequest, WithdrawFundsCommandResponse>
{
    public async Task<WithdrawFundsCommandResponse> Handle(WithdrawFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await unitOfWork.Wallets.GetByUserIdAsync(request.UserId)
            ?? throw new WalletNotFoundException(request.UserId);

        try
        {
            var transaction = wallet.Withdraw(request.Amount);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new WithdrawFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
        catch (InvalidOperationException ex) when (ex.Message == "INSUFFICIENT_BALANCE") { throw new InsufficientBalanceException(); }
    }
}

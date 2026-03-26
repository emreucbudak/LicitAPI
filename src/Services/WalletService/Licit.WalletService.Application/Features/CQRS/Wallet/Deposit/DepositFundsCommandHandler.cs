using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;

public class DepositFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<DepositFundsCommandRequest> validator) : IRequestHandler<DepositFundsCommandRequest, DepositFundsCommandResponse>
{
    public async Task<DepositFundsCommandResponse> Handle(DepositFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await unitOfWork.Wallets.GetByUserIdAsync(request.UserId);

        if (wallet is null)
        {
            wallet = new Domain.Entities.Wallet(request.UserId);
            unitOfWork.Wallets.Add(wallet);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var transaction = wallet.Deposit(request.Amount);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException();
        }

        return new DepositFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
    }
}

using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;

public class DepositFundsCommandHandler(
    IWalletRepository walletRepository,
    IValidator<DepositFundsCommandRequest> validator) : IRequestHandler<DepositFundsCommandRequest, DepositFundsCommandResponse>
{
    public async Task<DepositFundsCommandResponse> Handle(DepositFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId);

        if (wallet is null)
        {
            wallet = new Domain.Entities.Wallet(request.UserId);
            wallet = await walletRepository.CreateAsync(wallet);
        }

        var transaction = wallet.Deposit(request.Amount);

        try
        {
            await walletRepository.UpdateAsync(wallet);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException();
        }

        return new DepositFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
    }
}

using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Freeze.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;

public class FreezeFundsCommandHandler(
    IWalletRepository walletRepository,
    IValidator<FreezeFundsCommandRequest> validator) : IRequestHandler<FreezeFundsCommandRequest, FreezeFundsCommandResponse>
{
    public async Task<FreezeFundsCommandResponse> Handle(FreezeFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new WalletNotFoundException(request.UserId);

        try
        {
            var transaction = wallet.Freeze(request.Amount, request.ReferenceId, request.Description);
            await walletRepository.UpdateAsync(wallet);
            return new FreezeFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
        catch (InvalidOperationException ex) when (ex.Message == "INSUFFICIENT_BALANCE_FOR_FREEZE") { throw new InsufficientBalanceForFreezeException(); }
    }
}

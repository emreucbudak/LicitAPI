using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Freeze.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;

public class FreezeFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<FreezeFundsCommandRequest> validator) : IRequestHandler<FreezeFundsCommandRequest, FreezeFundsCommandResponse>
{
    public async Task<FreezeFundsCommandResponse> Handle(FreezeFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await unitOfWork.Wallets.GetByUserIdAsync(request.UserId)
            ?? throw new WalletNotFoundException(request.UserId);

        try
        {
            var transaction = wallet.Freeze(request.Amount, request.ReferenceId, request.Description);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new FreezeFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
    }
}

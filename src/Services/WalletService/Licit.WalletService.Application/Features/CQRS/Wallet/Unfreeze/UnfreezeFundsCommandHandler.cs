using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze;

public class UnfreezeFundsCommandHandler(
    IUnitOfWork unitOfWork,
    IValidator<UnfreezeFundsCommandRequest> validator) : IRequestHandler<UnfreezeFundsCommandRequest, UnfreezeFundsCommandResponse>
{
    public async Task<UnfreezeFundsCommandResponse> Handle(UnfreezeFundsCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await unitOfWork.Wallets.GetByUserIdAsync(request.UserId)
            ?? throw new WalletNotFoundException(request.UserId);

        try
        {
            var transaction = wallet.Unfreeze(request.Amount, request.ReferenceId, request.Description);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new UnfreezeFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
    }
}

using FlashMediator;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze;

public class UnfreezeFundsCommandHandler(
    IWalletRepository walletRepository) : IRequestHandler<UnfreezeFundsCommandRequest, UnfreezeFundsCommandResponse>
{
    public async Task<UnfreezeFundsCommandResponse> Handle(UnfreezeFundsCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Çözülecek tutar sıfırdan büyük olmalıdır.");

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Cüzdan bulunamadı.");

        if (wallet.FrozenBalance < request.Amount)
            throw new InvalidOperationException("Bloke edilmiş bakiye yetersiz.");

        wallet.FrozenBalance -= request.Amount;
        wallet.Balance += request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = TransactionType.Unfreeze,
            Amount = request.Amount,
            Description = request.Description ?? "İhale kaybedildi, bloke çözüldü",
            ReferenceId = request.ReferenceId,
            BalanceAfter = wallet.Balance,
            FrozenBalanceAfter = wallet.FrozenBalance
        };

        wallet.Transactions.Add(transaction);

        try
        {
            await walletRepository.UpdateAsync(wallet);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Eşzamanlılık hatası. Lütfen tekrar deneyin.");
        }

        return new UnfreezeFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
    }
}

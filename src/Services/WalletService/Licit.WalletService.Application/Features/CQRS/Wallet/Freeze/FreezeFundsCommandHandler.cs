using FlashMediator;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;

public class FreezeFundsCommandHandler(
    IWalletRepository walletRepository) : IRequestHandler<FreezeFundsCommandRequest, FreezeFundsCommandResponse>
{
    public async Task<FreezeFundsCommandResponse> Handle(FreezeFundsCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Bloke edilecek tutar sıfırdan büyük olmalıdır.");

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Cüzdan bulunamadı.");

        if (wallet.Balance < request.Amount)
            throw new InvalidOperationException("Yetersiz bakiye. Teklif vermek için yeterli kullanılabilir bakiye yok.");

        wallet.Balance -= request.Amount;
        wallet.FrozenBalance += request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = TransactionType.Freeze,
            Amount = request.Amount,
            Description = request.Description ?? "Teklif için bakiye bloke edildi",
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

        return new FreezeFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
    }
}

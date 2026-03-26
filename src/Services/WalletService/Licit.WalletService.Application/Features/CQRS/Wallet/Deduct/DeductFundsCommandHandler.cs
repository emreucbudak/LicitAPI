using FlashMediator;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deduct;

public class DeductFundsCommandHandler(
    IWalletRepository walletRepository) : IRequestHandler<DeductFundsCommandRequest, DeductFundsCommandResponse>
{
    public async Task<DeductFundsCommandResponse> Handle(DeductFundsCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Kesilecek tutar sıfırdan büyük olmalıdır.");

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Cüzdan bulunamadı.");

        if (wallet.FrozenBalance < request.Amount)
            throw new InvalidOperationException("Bloke edilmiş bakiye yetersiz. Kesim yapılamaz.");

        wallet.FrozenBalance -= request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = TransactionType.Deduct,
            Amount = request.Amount,
            Description = request.Description ?? "İhale kazanıldı, tutar kesildi",
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

        return new DeductFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
    }
}

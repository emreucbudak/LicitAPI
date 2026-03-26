using FlashMediator;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw;

public class WithdrawFundsCommandHandler(
    IWalletRepository walletRepository) : IRequestHandler<WithdrawFundsCommandRequest, WithdrawFundsCommandResponse>
{
    public async Task<WithdrawFundsCommandResponse> Handle(WithdrawFundsCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Çekilecek tutar sıfırdan büyük olmalıdır.");

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Cüzdan bulunamadı.");

        if (wallet.Balance < request.Amount)
            throw new InvalidOperationException("Yetersiz bakiye.");

        wallet.Balance -= request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = TransactionType.Withdraw,
            Amount = request.Amount,
            Description = "Para çekme",
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

        return new WithdrawFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
    }
}

using FlashMediator;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;

public class DepositFundsCommandHandler(
    IWalletRepository walletRepository) : IRequestHandler<DepositFundsCommandRequest, DepositFundsCommandResponse>
{
    public async Task<DepositFundsCommandResponse> Handle(DepositFundsCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Yatırılacak tutar sıfırdan büyük olmalıdır.");

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId);

        if (wallet is null)
        {
            wallet = new Domain.Entities.Wallet
            {
                UserId = request.UserId,
                Balance = 0,
                FrozenBalance = 0
            };
            wallet = await walletRepository.CreateAsync(wallet);
        }

        wallet.Balance += request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = TransactionType.Deposit,
            Amount = request.Amount,
            Description = "Para yatırma",
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

        return new DepositFundsCommandResponse(transaction.Id, wallet.Balance, wallet.FrozenBalance, transaction.CreatedAt);
    }
}

using Licit.WalletService.Domain.Common;

namespace Licit.WalletService.Domain.Entities;

public class WalletDepositPayment : BaseEntity
{
    public Guid UserId { get; private set; }
    public int Amount { get; private set; }
    public WalletDepositPaymentStatus Status { get; private set; }
    public string ClientIdempotencyKey { get; private set; } = null!;
    public string? StripePaymentIntentId { get; private set; }
    public Guid? WalletTransactionId { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public string Currency { get; private set; } = null!;

    private WalletDepositPayment() { }

    public WalletDepositPayment(Guid userId, int amount, string currency, string clientIdempotencyKey)
    {
        UserId = userId;
        Amount = amount;
        Currency = "try";
        ClientIdempotencyKey = clientIdempotencyKey;
        Status = WalletDepositPaymentStatus.Bekliyor;
    }

    public void RegisterPaymentIntent(string paymentIntentId)
    {
        StripePaymentIntentId = paymentIntentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSucceeded(Guid walletTransactionId)
    {
        Status = WalletDepositPaymentStatus.Başarılı;
        WalletTransactionId = walletTransactionId;
        ProcessedAt = DateTime.UtcNow;
        FailureCode = null;
        FailureMessage = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string? failureCode, string? failureMessage)
    {
        Status = WalletDepositPaymentStatus.Başarısız;
        FailureCode = failureCode;
        FailureMessage = failureMessage;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCanceled()
    {
        Status = WalletDepositPaymentStatus.İptal;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

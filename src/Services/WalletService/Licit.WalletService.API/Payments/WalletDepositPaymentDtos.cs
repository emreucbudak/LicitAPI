namespace Licit.WalletService.API.Payments;

public record CreateWalletDepositPaymentIntentRequest(decimal Amount);

public record WalletDepositPaymentIntentResponse(
    Guid DepositPaymentId,
    string PaymentIntentId,
    string ClientSecret,
    decimal Amount,
    string Currency);

public record WalletDepositPaymentSyncResponse(
    Guid DepositPaymentId,
    string PaymentIntentId,
    string Status,
    bool Applied,
    Guid? TransactionId,
    decimal Amount,
    string Currency);

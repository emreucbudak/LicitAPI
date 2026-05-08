namespace Licit.WalletService.API.Payments;

public class StripeWalletOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string Currency { get; set; } = "try";
    public decimal MinimumAmount { get; set; } = 1m;
    public decimal MaximumAmount { get; set; } = 100_000m;
}

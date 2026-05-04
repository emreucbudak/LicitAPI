namespace Licit.BiddingService.Application.DTOs
{
    public record WalletHoldResult(
        bool Success,
        Guid HoldId,
        long AvailableBalanceCents,
        long FrozenBalanceCents,
        bool IdempotentReplay,
        string? ErrorCode)
    {
        public static WalletHoldResult Held(
            Guid holdId,
            long availableBalanceCents,
            long frozenBalanceCents,
            bool idempotentReplay)
            => new(true, holdId, availableBalanceCents, frozenBalanceCents, idempotentReplay, null);

        public static WalletHoldResult Rejected(string errorCode)
            => new(false, Guid.Empty, 0, 0, false, errorCode);
    }
}

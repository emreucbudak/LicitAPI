namespace Licit.BiddingService.Infrastructure.Grpc
{
    public class WalletGrpcOptions
    {
        public const string SectionName = "WalletGrpc";

        public string Address { get; set; } = "http://localhost:5007";
        public string? ServiceKey { get; set; }
        public int DeadlineSeconds { get; set; } = 3;
    }
}

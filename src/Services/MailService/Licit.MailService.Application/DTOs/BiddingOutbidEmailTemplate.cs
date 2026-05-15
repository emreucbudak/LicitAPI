namespace Licit.MailService.Application.DTOs;

public static class BiddingOutbidEmailTemplate
{
    public static string BuildSubject()
        => "Teklif verdiğiniz ihalede yeni teklif var";

    public static string BuildBody(BiddingOutbidEmailEvent emailEvent)
        => $"""
           Merhaba,

           Takip ettiğiniz ihalede daha yüksek bir teklif verildi.

           İhale: {emailEvent.AuctionId}
           Yeni teklif: {emailEvent.Amount}
           Teklif tarihi: {emailEvent.PlacedAt:dd.MM.yyyy HH:mm}

           Tekrar öne geçmek isterseniz Licit üzerinden ihaleye dönebilirsiniz.
           """;
}

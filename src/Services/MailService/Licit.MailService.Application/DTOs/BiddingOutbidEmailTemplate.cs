namespace Licit.MailService.Application.DTOs;

public static class BiddingOutbidEmailTemplate
{
    public static string BuildSubject()
        => "Teklif verdiginiz ihalede yeni teklif var";

    public static string BuildBody(BiddingOutbidEmailEvent emailEvent)
        => $"""
           Merhaba,

           Takip ettiginiz ihalede yeni bir yuksek teklif verildi.

           Ihale: {emailEvent.AuctionId}
           Yeni teklif: {emailEvent.Amount}
           Teklif tarihi: {emailEvent.PlacedAt:dd.MM.yyyy HH:mm}

           Tekrar one gecmek isterseniz Licit uzerinden ihaleye donebilirsiniz.
           """;
}

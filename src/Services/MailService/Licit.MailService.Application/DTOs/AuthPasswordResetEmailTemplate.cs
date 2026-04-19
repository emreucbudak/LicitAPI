using System.Net;
using System.Text;

namespace Licit.MailService.Application.DTOs;

public static class AuthPasswordResetEmailTemplate
{
    public static string BuildSubject() => "Licit sifre sifirlama kodunuz";

    public static string BuildBody(AuthPasswordResetEmailEvent emailEvent)
    {
        var safeEmail = WebUtility.HtmlEncode(emailEvent.Email);
        var safeCode = WebUtility.HtmlEncode(emailEvent.Code);
        var safeUserName = string.IsNullOrWhiteSpace(emailEvent.UserName)
            ? string.Empty
            : $"<p>Merhaba {WebUtility.HtmlEncode(emailEvent.UserName)},</p>";
        var expiryText = emailEvent.ExpiresAt.HasValue
            ? $"<p>Kod, {emailEvent.ExpiresAt.Value:dd.MM.yyyy HH:mm} tarihine kadar gecerlidir.</p>"
            : string.Empty;

        var body = new StringBuilder();
        body.AppendLine("<html>");
        body.AppendLine("<body style=\"font-family:Arial,sans-serif;background-color:#f9fafb;padding:24px;color:#111827;\">");
        body.AppendLine("  <div style=\"max-width:560px;margin:0 auto;background:#ffffff;border-radius:16px;padding:32px;border:1px solid #e5e7eb;\">");
        body.AppendLine(safeUserName);
        body.AppendLine("    <p>Sifrenizi sifirlamak icin asagidaki 6 haneli dogrulama kodunu kullanin:</p>");
        body.AppendLine("    <div style=\"font-size:32px;font-weight:700;letter-spacing:6px;padding:16px 20px;border-radius:12px;background:#f3f4f6;text-align:center;margin:24px 0;\">");
        body.AppendLine(safeCode);
        body.AppendLine("    </div>");
        body.AppendLine(expiryText);
        body.AppendLine($"    <p>Bu kod <strong>{safeEmail}</strong> adresi icin olusturuldu.</p>");
        body.AppendLine("    <p>Eger bu sifre sifirlama talebini siz baslatmadiysaniz bu e-postayi yok sayabilirsiniz.</p>");
        body.AppendLine("  </div>");
        body.AppendLine("</body>");
        body.AppendLine("</html>");

        return body.ToString();
    }
}

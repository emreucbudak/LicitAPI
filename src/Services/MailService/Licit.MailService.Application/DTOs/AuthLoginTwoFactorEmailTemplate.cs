using System.Net;
using System.Text;

namespace Licit.MailService.Application.DTOs;

public static class AuthLoginTwoFactorEmailTemplate
{
    public static string BuildSubject() => "Licit giriş doğrulama kodunuz";

    public static string BuildBody(AuthLoginTwoFactorEmailEvent emailEvent)
    {
        var safeEmail = WebUtility.HtmlEncode(emailEvent.Email);
        var safeCode = WebUtility.HtmlEncode(emailEvent.Code);
        var greetingText = string.IsNullOrWhiteSpace(emailEvent.UserName)
            ? "Licit hesabina girisini tamamlamak icin asagidaki tek kullanimlik kodu kullan."
            : $"Merhaba {WebUtility.HtmlEncode(emailEvent.UserName)}, Licit hesabina girisini tamamlamak icin asagidaki tek kullanimlik kodu kullan.";
        var expiryText = emailEvent.ExpiresAt.HasValue
            ? $"Kod {emailEvent.ExpiresAt.Value:dd.MM.yyyy HH:mm} tarihine kadar gecerli."
            : "Kod kisa sure icin gecerli.";

        var body = new StringBuilder();
        body.AppendLine("<!doctype html>");
        body.AppendLine("<html>");
        body.AppendLine("<head>");
        body.AppendLine("  <meta charset=\"utf-8\">");
        body.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        body.AppendLine("  <title>Licit giris dogrulama kodu</title>");
        body.AppendLine("</head>");
        body.AppendLine("<body style=\"margin:0;padding:0;background:#0b1326;color:#f8fafc;font-family:Arial,Helvetica,sans-serif;\">");
        body.AppendLine("  <div style=\"display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;\">Licit giris kodun hazir.</div>");
        body.AppendLine("  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"width:100%;background:#0b1326;\">");
        body.AppendLine("    <tr>");
        body.AppendLine("      <td align=\"center\" style=\"padding:32px 16px;\">");
        body.AppendLine("        <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"width:100%;max-width:640px;border-collapse:separate;\">");
        body.AppendLine("          <tr>");
        body.AppendLine("            <td style=\"padding:0 0 18px 0;\">");
        body.AppendLine("              <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\">");
        body.AppendLine("                <tr>");
        body.AppendLine("                  <td style=\"font-size:24px;font-weight:900;letter-spacing:-0.03em;color:#ffffff;\">Licit</td>");
        body.AppendLine("                  <td align=\"right\" style=\"font-size:12px;font-weight:800;letter-spacing:0.16em;text-transform:uppercase;color:#4edea3;\">Guvenli Giris</td>");
        body.AppendLine("                </tr>");
        body.AppendLine("              </table>");
        body.AppendLine("            </td>");
        body.AppendLine("          </tr>");
        body.AppendLine("          <tr>");
        body.AppendLine("            <td style=\"overflow:hidden;border-radius:24px;background:#111c31;border:1px solid #2d3449;box-shadow:0 28px 70px rgba(0,0,0,0.32);\">");
        body.AppendLine("              <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\">");
        body.AppendLine("                <tr>");
        body.AppendLine("                  <td style=\"height:6px;background:#494bd6;font-size:0;line-height:0;\">&nbsp;</td>");
        body.AppendLine("                </tr>");
        body.AppendLine("                <tr>");
        body.AppendLine("                  <td style=\"padding:34px 34px 12px 34px;\">");
        body.AppendLine("                    <div style=\"display:inline-block;padding:8px 12px;border-radius:999px;background:#1f2a44;color:#c0c1ff;font-size:12px;font-weight:800;letter-spacing:0.14em;text-transform:uppercase;\">Tek kullanimlik kod</div>");
        body.AppendLine("                    <h1 style=\"margin:20px 0 12px 0;color:#ffffff;font-size:30px;line-height:1.18;font-weight:900;letter-spacing:-0.04em;\">Giris kodun hazir</h1>");
        body.AppendLine($"                    <p style=\"margin:0;color:#cbd5e1;font-size:16px;line-height:1.7;\">{greetingText}</p>");
        body.AppendLine("                  </td>");
        body.AppendLine("                </tr>");
        body.AppendLine("                <tr>");
        body.AppendLine("                  <td style=\"padding:18px 34px 8px 34px;\">");
        body.AppendLine("                    <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#0b1326;border:1px solid #3b4261;border-radius:18px;\">");
        body.AppendLine("                      <tr>");
        body.AppendLine("                        <td align=\"center\" style=\"padding:24px 18px 8px 18px;color:#94a3b8;font-size:12px;font-weight:800;letter-spacing:0.16em;text-transform:uppercase;\">Dogrulama kodu</td>");
        body.AppendLine("                      </tr>");
        body.AppendLine("                      <tr>");
        body.AppendLine($"                        <td align=\"center\" style=\"padding:0 18px 26px 18px;color:#4edea3;font-family:'Courier New',Courier,monospace;font-size:42px;line-height:1;font-weight:900;letter-spacing:10px;\">{safeCode}</td>");
        body.AppendLine("                      </tr>");
        body.AppendLine("                    </table>");
        body.AppendLine("                  </td>");
        body.AppendLine("                </tr>");
        body.AppendLine("                <tr>");
        body.AppendLine("                  <td style=\"padding:10px 34px 26px 34px;\">");
        body.AppendLine("                    <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\">");
        body.AppendLine("                      <tr>");
        body.AppendLine("                        <td style=\"padding:14px 16px;border-radius:14px;background:#1a2438;color:#dbeafe;font-size:14px;line-height:1.55;border:1px solid #2d3449;\">");
        body.AppendLine($"                          <strong style=\"color:#ffffff;\">Sure:</strong> {expiryText}");
        body.AppendLine("                        </td>");
        body.AppendLine("                      </tr>");
        body.AppendLine("                    </table>");
        body.AppendLine("                  </td>");
        body.AppendLine("                </tr>");
        body.AppendLine("                <tr>");
        body.AppendLine("                  <td style=\"padding:0 34px 34px 34px;\">");
        body.AppendLine($"                    <p style=\"margin:0 0 10px 0;color:#94a3b8;font-size:14px;line-height:1.65;\">Bu kod <strong style=\"color:#f8fafc;\">{safeEmail}</strong> adresi icin olusturuldu.</p>");
        body.AppendLine("                    <p style=\"margin:0;color:#94a3b8;font-size:13px;line-height:1.65;\">Bu girisi sen baslatmadiysan e-postayi yok sayabilir, hesabini guvende tutmak icin sifreni degistirebilirsin.</p>");
        body.AppendLine("                  </td>");
        body.AppendLine("                </tr>");
        body.AppendLine("              </table>");
        body.AppendLine("            </td>");
        body.AppendLine("          </tr>");
        body.AppendLine("          <tr>");
        body.AppendLine("            <td align=\"center\" style=\"padding:18px 10px 0 10px;color:#64748b;font-size:12px;line-height:1.5;\">Licit guvenli teklif deneyimi icin bu kodu kimseyle paylasma.</td>");
        body.AppendLine("          </tr>");
        body.AppendLine("        </table>");
        body.AppendLine("      </td>");
        body.AppendLine("    </tr>");
        body.AppendLine("  </table>");
        body.AppendLine("</body>");
        body.AppendLine("</html>");

        return body.ToString();
    }
}

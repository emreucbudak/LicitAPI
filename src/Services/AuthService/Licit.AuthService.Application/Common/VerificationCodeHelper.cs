using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Licit.AuthService.Application.Common;

public static class VerificationCodeHelper
{
    public static string GenerateSixDigitCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);

    public static bool CodesMatch(string expectedCode, string actualCode)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expectedCode);
        var actualBytes = Encoding.UTF8.GetBytes(actualCode);

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}

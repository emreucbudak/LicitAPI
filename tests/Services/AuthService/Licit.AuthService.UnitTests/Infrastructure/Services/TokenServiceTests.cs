using FluentAssertions;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Infrastructure.Services;
using Licit.AuthService.UnitTests.Common;

namespace Licit.AuthService.UnitTests.Infrastructure.Services;

public class TokenServiceTests
{
    private readonly JwtSettings _jwtSettings = new()
    {
        Secret = "this-is-a-test-secret-with-enough-length-for-hmac-sha256",
        Issuer = "licit-auth-tests",
        Audience = "licit-auth-tests",
        RefreshTokenExpirationDays = 7
    };

    [Fact]
    public async Task ValidateRefreshToken_AfterRevoke_ShouldReturnNull()
    {
        var userId = Guid.NewGuid();
        var tokenService = new TokenService(
            _jwtSettings,
            UserManagerMockHelper.CreateMock(),
            new InMemoryDistributedCache());
        var refreshToken = tokenService.GenerateRefreshToken(userId);

        tokenService.ValidateRefreshToken(refreshToken).Should().Be(userId);

        await tokenService.RevokeRefreshTokenAsync(refreshToken);

        tokenService.ValidateRefreshToken(refreshToken).Should().BeNull();
    }
}

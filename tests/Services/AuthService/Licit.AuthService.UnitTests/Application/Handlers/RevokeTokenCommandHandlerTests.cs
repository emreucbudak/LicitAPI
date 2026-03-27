using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;
using Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken.Exceptions;
using Licit.AuthService.Application.Interfaces;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class RevokeTokenCommandHandlerTests
{
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IValidator<RevokeTokenCommandRequest> _validator = Substitute.For<IValidator<RevokeTokenCommandRequest>>();
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<RevokeTokenCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new RevokeTokenCommandHandler(_tokenService, _validator);
    }

    [Fact]
    public async Task Handle_ValidToken_ShouldNotThrow()
    {
        _tokenService.ValidateRefreshToken("valid-token").Returns(Guid.NewGuid());

        var act = () => _handler.Handle(new RevokeTokenCommandRequest("valid-token"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_InvalidToken_ShouldThrowInvalidTokenException()
    {
        _tokenService.ValidateRefreshToken("bad-token").Returns((Guid?)null);

        var act = () => _handler.Handle(new RevokeTokenCommandRequest("bad-token"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidTokenException>();
    }
}

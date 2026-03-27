using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Register;
using Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class RegisterCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly JwtSettings _jwtSettings = new() { Secret = "test", Issuer = "test", Audience = "test", AccessTokenExpirationMinutes = 15 };
    private readonly IValidator<RegisterCommandRequest> _validator = Substitute.For<IValidator<RegisterCommandRequest>>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<RegisterCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new RegisterCommandHandler(_userManager, _tokenService, _jwtSettings, _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateUserAndReturnTokens()
    {
        _userManager.FindByEmailAsync("test@test.com").Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), "Password123!").Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "User").Returns(IdentityResult.Success);
        _tokenService.GenerateAccessTokenAsync(Arg.Any<ApplicationUser>()).Returns("access-token");
        _tokenService.GenerateRefreshToken(Arg.Any<Guid>()).Returns("refresh-token");

        var result = await _handler.Handle(new RegisterCommandRequest("test@test.com", "Password123!", "Ali", "Veli"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        await _userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), "Password123!");
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "User");
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldThrow()
    {
        var existingUser = new ApplicationUser { Email = "test@test.com" };
        _userManager.FindByEmailAsync("test@test.com").Returns(existingUser);

        var act = () => _handler.Handle(new RegisterCommandRequest("test@test.com", "Password123!", "Ali", "Veli"), CancellationToken.None);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
    }

    [Fact]
    public async Task Handle_CreateFails_ShouldThrowUserCreationFailed()
    {
        _userManager.FindByEmailAsync("test@test.com").Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Şifre zayıf" }));

        var act = () => _handler.Handle(new RegisterCommandRequest("test@test.com", "weak", "Ali", "Veli"), CancellationToken.None);

        await act.Should().ThrowAsync<UserCreationFailedException>();
    }
}

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetBalance;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.UnitTests.Common;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class GetBalanceQueryHandlerTests
{
    private readonly IWalletProvisioningService _walletProvisioningService = Substitute.For<IWalletProvisioningService>();
    private readonly IValidator<GetBalanceQueryRequest> _validator = Substitute.For<IValidator<GetBalanceQueryRequest>>();
    private readonly GetBalanceQueryHandler _handler;

    public GetBalanceQueryHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<GetBalanceQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new GetBalanceQueryHandler(_walletProvisioningService, _validator);
    }

    [Fact]
    public async Task Handle_WalletExists_ShouldReturnBalances()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(700, 300);
        _walletProvisioningService.EnsureWalletExistsAsync(wallet.UserId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _handler.Handle(new GetBalanceQueryRequest(wallet.UserId), CancellationToken.None);

        result.Balance.Should().Be(700);
        result.FrozenBalance.Should().Be(300);
        result.TotalBalance.Should().Be(1000);
    }

    [Fact]
    public async Task Handle_NewWallet_ShouldReturnProvisionedWalletBalances()
    {
        var userId = Guid.NewGuid();
        var wallet = WalletTestFactory.CreateEmptyWallet(userId);
        _walletProvisioningService.EnsureWalletExistsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(wallet);

        var result = await _handler.Handle(new GetBalanceQueryRequest(userId), CancellationToken.None);

        result.Balance.Should().Be(0);
        result.FrozenBalance.Should().Be(0);
        result.TotalBalance.Should().Be(0);
    }
}

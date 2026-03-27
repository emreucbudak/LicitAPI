using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class GetBalanceQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IValidator<GetBalanceQueryRequest> _validator = Substitute.For<IValidator<GetBalanceQueryRequest>>();
    private readonly GetBalanceQueryHandler _handler;

    public GetBalanceQueryHandlerTests()
    {
        _unitOfWork.Wallets.Returns(_walletRepo);
        _validator.ValidateAsync(Arg.Any<GetBalanceQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new GetBalanceQueryHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_WalletExists_ShouldReturnBalances()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(700m, 300m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var result = await _handler.Handle(new GetBalanceQueryRequest(wallet.UserId), CancellationToken.None);

        result.Balance.Should().Be(700m);
        result.FrozenBalance.Should().Be(300m);
        result.TotalBalance.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_WalletNotFound_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        _walletRepo.GetByUserIdAsync(userId).Returns((Wallet?)null);

        var act = () => _handler.Handle(new GetBalanceQueryRequest(userId), CancellationToken.None);

        await act.Should().ThrowAsync<WalletNotFoundForBalanceException>();
    }
}

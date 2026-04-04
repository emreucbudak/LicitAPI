using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Deduct;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Exceptions;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class DeductFundsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IValidator<DeductFundsCommandRequest> _validator = Substitute.For<IValidator<DeductFundsCommandRequest>>();
    private readonly DeductFundsCommandHandler _handler;

    public DeductFundsCommandHandlerTests()
    {
        _unitOfWork.Wallets.Returns(_walletRepo);
        _validator.ValidateAsync(Arg.Any<DeductFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new DeductFundsCommandHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldDeductAndReturn()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 300m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var result = await _handler.Handle(new DeductFundsCommandRequest(wallet.UserId, 300m, Guid.NewGuid(), null), CancellationToken.None);

        result.AvailableBalance.Should().Be(500m);
        result.FrozenBalance.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WalletNotFound_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        _walletRepo.GetByUserIdAsync(userId).Returns((Wallet?)null);

        var act = () => _handler.Handle(new DeductFundsCommandRequest(userId, 100m, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<WalletNotFoundException>();
    }

    [Fact]
    public async Task Handle_InsufficientFrozenBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 50m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var act = () => _handler.Handle(new DeductFundsCommandRequest(wallet.UserId, 100m, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<InsufficientFrozenBalanceException>();
    }
}

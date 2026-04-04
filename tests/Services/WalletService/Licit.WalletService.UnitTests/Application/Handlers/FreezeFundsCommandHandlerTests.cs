using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class FreezeFundsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IValidator<FreezeFundsCommandRequest> _validator = Substitute.For<IValidator<FreezeFundsCommandRequest>>();
    private readonly FreezeFundsCommandHandler _handler;

    public FreezeFundsCommandHandlerTests()
    {
        _unitOfWork.Wallets.Returns(_walletRepo);
        _validator.ValidateAsync(Arg.Any<FreezeFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new FreezeFundsCommandHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldFreezeAndReturn()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);
        var refId = Guid.NewGuid();

        var result = await _handler.Handle(new FreezeFundsCommandRequest(wallet.UserId, 300m, refId, null), CancellationToken.None);

        result.AvailableBalance.Should().Be(700m);
        result.FrozenBalance.Should().Be(300m);
    }

    [Fact]
    public async Task Handle_WalletNotFound_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        _walletRepo.GetByUserIdAsync(userId).Returns((Wallet?)null);

        var act = () => _handler.Handle(new FreezeFundsCommandRequest(userId, 100m, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<WalletNotFoundException>();
    }

    [Fact]
    public async Task Handle_InsufficientBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(50m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var act = () => _handler.Handle(new FreezeFundsCommandRequest(wallet.UserId, 100m, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<Licit.WalletService.Domain.Exceptions.InsufficientBalanceException>();
    }
}

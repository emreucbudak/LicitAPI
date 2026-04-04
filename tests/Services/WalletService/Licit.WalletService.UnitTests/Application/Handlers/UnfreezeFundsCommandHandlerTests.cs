using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using Licit.WalletService.Domain.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class UnfreezeFundsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IValidator<UnfreezeFundsCommandRequest> _validator = Substitute.For<IValidator<UnfreezeFundsCommandRequest>>();
    private readonly UnfreezeFundsCommandHandler _handler;

    public UnfreezeFundsCommandHandlerTests()
    {
        _unitOfWork.Wallets.Returns(_walletRepo);
        _validator.ValidateAsync(Arg.Any<UnfreezeFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new UnfreezeFundsCommandHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUnfreezeAndReturn()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(700m, 300m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var result = await _handler.Handle(new UnfreezeFundsCommandRequest(wallet.UserId, 300m, Guid.NewGuid(), null), CancellationToken.None);

        result.AvailableBalance.Should().Be(1000m);
        result.FrozenBalance.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WalletNotFound_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        _walletRepo.GetByUserIdAsync(userId).Returns((Wallet?)null);

        var act = () => _handler.Handle(new UnfreezeFundsCommandRequest(userId, 100m, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<WalletNotFoundException>();
    }

    [Fact]
    public async Task Handle_InsufficientFrozenBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 50m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var act = () => _handler.Handle(new UnfreezeFundsCommandRequest(wallet.UserId, 100m, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<InsufficientFrozenBalanceException>();
    }
}

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class WithdrawFundsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IValidator<WithdrawFundsCommandRequest> _validator = Substitute.For<IValidator<WithdrawFundsCommandRequest>>();
    private readonly WithdrawFundsCommandHandler _handler;

    public WithdrawFundsCommandHandlerTests()
    {
        _unitOfWork.Wallets.Returns(_walletRepo);
        _validator.ValidateAsync(Arg.Any<WithdrawFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new WithdrawFundsCommandHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldWithdrawAndReturn()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var result = await _handler.Handle(new WithdrawFundsCommandRequest(wallet.UserId, 300m), CancellationToken.None);

        result.NewBalance.Should().Be(700m);
    }

    [Fact]
    public async Task Handle_WalletNotFound_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        _walletRepo.GetByUserIdAsync(userId).Returns((Wallet?)null);

        var act = () => _handler.Handle(new WithdrawFundsCommandRequest(userId, 100m), CancellationToken.None);

        await act.Should().ThrowAsync<WalletNotFoundException>();
    }

    [Fact]
    public async Task Handle_InsufficientBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(50m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var act = () => _handler.Handle(new WithdrawFundsCommandRequest(wallet.UserId, 100m), CancellationToken.None);

        await act.Should().ThrowAsync<Licit.WalletService.Domain.Exceptions.InsufficientBalanceException>();
    }
}

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class DepositFundsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IValidator<DepositFundsCommandRequest> _validator = Substitute.For<IValidator<DepositFundsCommandRequest>>();
    private readonly DepositFundsCommandHandler _handler;

    public DepositFundsCommandHandlerTests()
    {
        _unitOfWork.Wallets.Returns(_walletRepo);
        _validator.ValidateAsync(Arg.Any<DepositFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new DepositFundsCommandHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_ExistingWallet_ShouldDepositAndReturn()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(500m);
        var userId = wallet.UserId;
        _walletRepo.GetByUserIdAsync(userId).Returns(wallet);

        var result = await _handler.Handle(new DepositFundsCommandRequest(userId, 200m), CancellationToken.None);

        result.NewBalance.Should().Be(700m);
        await _unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoWallet_ShouldCreateWalletAndDeposit()
    {
        var userId = Guid.NewGuid();
        _walletRepo.GetByUserIdAsync(userId).Returns((Wallet?)null);

        var result = await _handler.Handle(new DepositFundsCommandRequest(userId, 100m), CancellationToken.None);

        result.NewBalance.Should().Be(100m);
        _walletRepo.Received(1).Add(Arg.Any<Wallet>());
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<DepositFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Amount", "Hata") }));

        var act = () => _handler.Handle(new DepositFundsCommandRequest(Guid.NewGuid(), 100m), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}

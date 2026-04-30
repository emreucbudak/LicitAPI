using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw.Exceptions;
using Licit.WalletService.Domain.Exceptions;
using Licit.WalletService.Application.Interfaces;
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
        _walletRepo.GetTransactionByWalletTypeAndReferenceAsync(Arg.Any<Guid>(), Arg.Any<TransactionType>(), Arg.Any<Guid>())
            .Returns(Task.FromResult<WalletTransaction?>(null));
        _validator.ValidateAsync(Arg.Any<DeductFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new DeductFundsCommandHandler(_unitOfWork, _walletRepo, _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldDeductAndReturn()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 300m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var result = await _handler.Handle(new DeductFundsCommandRequest(wallet.UserId, 300m, Guid.NewGuid(), null), CancellationToken.None);

        result.AvailableBalance.Should().Be(500m);
        result.FrozenBalance.Should().Be(0m);
        result.IdempotentReplay.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DuplicateReference_ShouldReturnExistingTransactionWithoutDeductingAgain()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 300m);
        var refId = Guid.NewGuid();
        var existingTransaction = new WalletTransaction(
            wallet.Id,
            TransactionType.Deduct,
            300m,
            "existing deduct",
            refId,
            500m,
            0m);

        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);
        _walletRepo.GetTransactionByWalletTypeAndReferenceAsync(wallet.Id, TransactionType.Deduct, refId)
            .Returns(Task.FromResult<WalletTransaction?>(existingTransaction));

        var result = await _handler.Handle(new DeductFundsCommandRequest(wallet.UserId, 300m, refId, null), CancellationToken.None);

        result.TransactionId.Should().Be(existingTransaction.Id);
        result.AvailableBalance.Should().Be(500m);
        result.FrozenBalance.Should().Be(0m);
        result.IdempotentReplay.Should().BeTrue();
        wallet.Balance.Should().Be(500m);
        wallet.FrozenBalance.Should().Be(300m);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
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

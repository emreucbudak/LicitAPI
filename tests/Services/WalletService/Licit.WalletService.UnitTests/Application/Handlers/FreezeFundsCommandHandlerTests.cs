using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw.Exceptions;
using DomainExceptions = Licit.WalletService.Domain.Exceptions;
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
        _walletRepo.GetTransactionByWalletTypeAndReferenceAsync(Arg.Any<Guid>(), Arg.Any<TransactionType>(), Arg.Any<Guid>())
            .Returns(Task.FromResult<WalletTransaction?>(null));
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
        result.IdempotentReplay.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DuplicateReference_ShouldReturnExistingTransactionWithoutFreezingAgain()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000m);
        var refId = Guid.NewGuid();
        var existingTransaction = new WalletTransaction(
            wallet.Id,
            TransactionType.Freeze,
            300m,
            "existing freeze",
            refId,
            700m,
            300m);

        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);
        _walletRepo.GetTransactionByWalletTypeAndReferenceAsync(wallet.Id, TransactionType.Freeze, refId)
            .Returns(Task.FromResult<WalletTransaction?>(existingTransaction));

        var result = await _handler.Handle(new FreezeFundsCommandRequest(wallet.UserId, 300m, refId, null), CancellationToken.None);

        result.TransactionId.Should().Be(existingTransaction.Id);
        result.AvailableBalance.Should().Be(700m);
        result.FrozenBalance.Should().Be(300m);
        result.IdempotentReplay.Should().BeTrue();
        wallet.Balance.Should().Be(1000m);
        wallet.FrozenBalance.Should().Be(0m);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
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

        await act.Should().ThrowAsync<DomainExceptions.InsufficientBalanceException>();
    }
}

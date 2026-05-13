using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit.Exceptions;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class DepositFundsCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUnitOfWorkTransaction _unitOfWorkTransaction = Substitute.For<IUnitOfWorkTransaction>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IDepositIdempotencyStore _idempotencyStore = Substitute.For<IDepositIdempotencyStore>();
    private readonly IValidator<DepositFundsCommandRequest> _validator = Substitute.For<IValidator<DepositFundsCommandRequest>>();
    private readonly DepositFundsCommandHandler _handler;

    public DepositFundsCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<DepositFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _idempotencyStore.TryReserveAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(true);
        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(_unitOfWorkTransaction);
        _handler = new DepositFundsCommandHandler(_unitOfWork, _walletRepo, _idempotencyStore, _validator);
    }

    [Fact]
    public async Task Handle_ExistingWallet_ShouldDepositAndReturn()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(500);
        var userId = wallet.UserId;
        _walletRepo.GetByUserIdAsync(userId).Returns(wallet);

        var result = await _handler.Handle(new DepositFundsCommandRequest(userId, 200, "deposit-key-1"), CancellationToken.None);

        result.NewBalance.Should().Be(700);
        await _idempotencyStore.Received(1).TryReserveAsync(
            userId,
            "deposit-key-1",
            TimeSpan.FromMinutes(2),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoWallet_ShouldCreateWalletAndDeposit()
    {
        var userId = Guid.NewGuid();
        _walletRepo.GetByUserIdAsync(userId).Returns((Wallet?)null);

        var result = await _handler.Handle(new DepositFundsCommandRequest(userId, 100, "deposit-key-2"), CancellationToken.None);

        result.NewBalance.Should().Be(100);
        _walletRepo.Received(1).Add(Arg.Any<Wallet>());
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<DepositFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Amount", "Hata") }));

        var act = () => _handler.Handle(new DepositFundsCommandRequest(Guid.NewGuid(), 100, "deposit-key-3"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await _idempotencyStore.DidNotReceive().TryReserveAsync(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateIdempotencyKey_ShouldThrowAndNotDeposit()
    {
        var userId = Guid.NewGuid();
        _idempotencyStore.TryReserveAsync(
                userId,
                "same-key",
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _handler.Handle(new DepositFundsCommandRequest(userId, 100, "same-key"), CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateDepositRequestException>();
        await _walletRepo.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaveFails_ShouldReleaseIdempotencyKey()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(500);
        var userId = wallet.UserId;
        _walletRepo.GetByUserIdAsync(userId).Returns(wallet);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new DbUpdateConcurrencyException()));

        var act = () => _handler.Handle(new DepositFundsCommandRequest(userId, 100, "retry-key"), CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyException>();
        await _idempotencyStore.Received(1).ReleaseAsync(userId, "retry-key", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConcurrencyWithExistingReference_ShouldReturnExistingTransaction()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(500);
        var refId = Guid.NewGuid();
        var existingTransaction = new WalletTransaction(
            wallet.Id,
            TransactionType.Deposit,
            100,
            "existing deposit",
            refId,
            600,
            0);

        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);
        _walletRepo.GetTransactionByWalletTypeAndReferenceAsync(wallet.Id, TransactionType.Deposit, refId)
            .Returns(
                Task.FromResult<WalletTransaction?>(null),
                Task.FromResult<WalletTransaction?>(existingTransaction));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new DbUpdateConcurrencyException()));

        var result = await _handler.Handle(
            new DepositFundsCommandRequest(wallet.UserId, 100, "stripe-key", refId),
            CancellationToken.None);

        result.TransactionId.Should().Be(existingTransaction.Id);
        result.NewBalance.Should().Be(600);
        await _idempotencyStore.DidNotReceive().ReleaseAsync(wallet.UserId, "stripe-key", Arg.Any<CancellationToken>());
    }
}

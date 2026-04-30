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
        _handler = new DepositFundsCommandHandler(_unitOfWork, _walletRepo, _idempotencyStore, _validator);
    }

    [Fact]
    public async Task Handle_ExistingWallet_ShouldDepositAndReturn()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(500m);
        var userId = wallet.UserId;
        _walletRepo.GetByUserIdAsync(userId).Returns(wallet);

        var result = await _handler.Handle(new DepositFundsCommandRequest(userId, 200m, "deposit-key-1"), CancellationToken.None);

        result.NewBalance.Should().Be(700m);
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

        var result = await _handler.Handle(new DepositFundsCommandRequest(userId, 100m, "deposit-key-2"), CancellationToken.None);

        result.NewBalance.Should().Be(100m);
        _walletRepo.Received(1).Add(Arg.Any<Wallet>());
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<DepositFundsCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Amount", "Hata") }));

        var act = () => _handler.Handle(new DepositFundsCommandRequest(Guid.NewGuid(), 100m, "deposit-key-3"), CancellationToken.None);

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

        var act = () => _handler.Handle(new DepositFundsCommandRequest(userId, 100m, "same-key"), CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateDepositRequestException>();
        await _walletRepo.DidNotReceive().GetByUserIdAsync(Arg.Any<Guid>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaveFails_ShouldReleaseIdempotencyKey()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(500m);
        var userId = wallet.UserId;
        _walletRepo.GetByUserIdAsync(userId).Returns(wallet);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new DbUpdateConcurrencyException()));

        var act = () => _handler.Handle(new DepositFundsCommandRequest(userId, 100m, "retry-key"), CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyException>();
        await _idempotencyStore.Received(1).ReleaseAsync(userId, "retry-key", Arg.Any<CancellationToken>());
    }
}

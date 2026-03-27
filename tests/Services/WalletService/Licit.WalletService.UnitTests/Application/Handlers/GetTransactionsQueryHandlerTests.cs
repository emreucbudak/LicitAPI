using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;
using NSubstitute;

namespace Licit.WalletService.UnitTests.Application.Handlers;

public class GetTransactionsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IWalletRepository _walletRepo = Substitute.For<IWalletRepository>();
    private readonly IValidator<GetTransactionsQueryRequest> _validator = Substitute.For<IValidator<GetTransactionsQueryRequest>>();
    private readonly GetTransactionsQueryHandler _handler;

    public GetTransactionsQueryHandlerTests()
    {
        _unitOfWork.Wallets.Returns(_walletRepo);
        _validator.ValidateAsync(Arg.Any<GetTransactionsQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new GetTransactionsQueryHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_WalletExists_ShouldReturnTransactions()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000m);
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);

        var tx = new WalletTransaction(wallet.Id, TransactionType.Deposit, 1000m, "Yatırma", null, 1000m, 0m);
        _walletRepo.GetTransactionsByWalletIdAsync(wallet.Id, 1, 20).Returns(new List<WalletTransaction> { tx });

        var result = await _handler.Handle(new GetTransactionsQueryRequest(wallet.UserId, 1, 20), CancellationToken.None);

        result.Transactions.Should().HaveCount(1);
        result.Transactions[0].Type.Should().Be("Deposit");
    }

    [Fact]
    public async Task Handle_WalletNotFound_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        _walletRepo.GetByUserIdAsync(userId).Returns((Wallet?)null);

        var act = () => _handler.Handle(new GetTransactionsQueryRequest(userId, 1, 20), CancellationToken.None);

        await act.Should().ThrowAsync<WalletNotFoundForTransactionsException>();
    }

    [Fact]
    public async Task Handle_NoTransactions_ShouldReturnEmptyList()
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();
        _walletRepo.GetByUserIdAsync(wallet.UserId).Returns(wallet);
        _walletRepo.GetTransactionsByWalletIdAsync(wallet.Id, 1, 20).Returns(Enumerable.Empty<WalletTransaction>());

        var result = await _handler.Handle(new GetTransactionsQueryRequest(wallet.UserId, 1, 20), CancellationToken.None);

        result.Transactions.Should().BeEmpty();
    }
}

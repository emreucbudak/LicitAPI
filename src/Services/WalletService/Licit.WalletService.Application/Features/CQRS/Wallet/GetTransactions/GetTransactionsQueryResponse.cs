namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions;

public record GetTransactionsQueryResponse(
    List<TransactionDto> Transactions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);

public record TransactionDto(
    Guid Id,
    string Type,
    decimal Amount,
    string Description,
    Guid? ReferenceId,
    decimal BalanceAfter,
    decimal FrozenBalanceAfter,
    DateTime CreatedAt
);

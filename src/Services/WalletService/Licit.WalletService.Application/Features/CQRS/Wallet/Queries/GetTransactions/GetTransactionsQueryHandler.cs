using FlashMediator;
using FluentValidation;
using Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetTransactions.Exceptions;
using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetTransactions;

public class GetTransactionsQueryHandler(
    IWalletRepository walletRepository,
    IValidator<GetTransactionsQueryRequest> validator) : IRequestHandler<GetTransactionsQueryRequest, GetTransactionsQueryResponse>
{
    public async Task<GetTransactionsQueryResponse> Handle(GetTransactionsQueryRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var wallet = await walletRepository.GetByUserIdAsync(request.UserId)
            ?? throw new WalletNotFoundForTransactionsException(request.UserId);

        var totalCount = await walletRepository.GetTransactionCountByWalletIdAsync(wallet.Id);
        var transactions = await walletRepository.GetTransactionsByWalletIdAsync(wallet.Id, request.Page, request.PageSize);

        var dtos = transactions.Select(t => new TransactionDto(
            t.Id,
            t.Type.ToString(),
            t.Amount,
            t.Description,
            t.ReferenceId,
            t.BalanceAfter,
            t.FrozenBalanceAfter,
            t.CreatedAt
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new GetTransactionsQueryResponse(
            dtos, totalCount, request.Page, request.PageSize,
            totalPages, request.Page < totalPages, request.Page > 1);
    }
}

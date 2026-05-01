using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetBalance;

public record GetCurrentUserBalanceQueryRequest : IRequest<GetBalanceQueryResponse>;

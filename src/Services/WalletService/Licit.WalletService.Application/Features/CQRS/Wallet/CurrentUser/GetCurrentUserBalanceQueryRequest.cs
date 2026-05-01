using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetBalance;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public record GetCurrentUserBalanceQueryRequest : IRequest<GetBalanceQueryResponse>;

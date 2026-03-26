using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance;

public record GetBalanceQueryRequest(
    Guid UserId
) : IRequest<GetBalanceQueryResponse>;

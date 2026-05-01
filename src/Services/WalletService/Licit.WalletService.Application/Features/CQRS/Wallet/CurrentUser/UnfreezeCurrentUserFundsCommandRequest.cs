using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public record UnfreezeCurrentUserFundsCommandRequest(
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<UnfreezeFundsCommandResponse>;

using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze;

public record UnfreezeCurrentUserFundsCommandRequest(
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<UnfreezeFundsCommandResponse>;

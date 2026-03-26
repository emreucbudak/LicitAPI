using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze;

public record UnfreezeFundsCommandRequest(
    Guid UserId,
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<UnfreezeFundsCommandResponse>;

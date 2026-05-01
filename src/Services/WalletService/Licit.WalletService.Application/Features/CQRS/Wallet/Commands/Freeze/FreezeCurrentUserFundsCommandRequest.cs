using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze;

public record FreezeCurrentUserFundsCommandRequest(
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<FreezeFundsCommandResponse>;

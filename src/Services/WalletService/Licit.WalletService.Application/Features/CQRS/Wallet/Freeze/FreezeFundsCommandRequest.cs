using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;

public record FreezeFundsCommandRequest(
    Guid UserId,
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<FreezeFundsCommandResponse>;

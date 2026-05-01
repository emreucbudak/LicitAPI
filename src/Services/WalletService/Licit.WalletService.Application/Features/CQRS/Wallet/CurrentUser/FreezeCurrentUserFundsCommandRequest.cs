using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public record FreezeCurrentUserFundsCommandRequest(
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<FreezeFundsCommandResponse>;

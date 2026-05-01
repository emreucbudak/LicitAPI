using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.CurrentUser;

public record DeductCurrentUserFundsCommandRequest(
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<DeductFundsCommandResponse>;

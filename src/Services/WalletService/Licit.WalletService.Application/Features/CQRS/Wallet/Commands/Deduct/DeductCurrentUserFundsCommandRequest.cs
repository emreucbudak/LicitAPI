using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;

public record DeductCurrentUserFundsCommandRequest(
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<DeductFundsCommandResponse>;

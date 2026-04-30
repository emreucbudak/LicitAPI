using FlashMediator;

namespace Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;

public record DeductFundsCommandRequest(
    Guid UserId,
    decimal Amount,
    Guid ReferenceId,
    string? Description
) : IRequest<DeductFundsCommandResponse>;

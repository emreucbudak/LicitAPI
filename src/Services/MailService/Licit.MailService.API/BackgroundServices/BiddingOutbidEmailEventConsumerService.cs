using DotNetCore.CAP;
using FlashMediator;
using Licit.MailService.API.Integrations;
using Licit.MailService.Application.DTOs;
using Licit.MailService.Application.Features.CQRS.Email.Commands.Send;

namespace Licit.MailService.API.BackgroundServices;

public class BiddingOutbidEmailEventConsumerService(
    AuthUserEmailLookupClient emailLookupClient,
    IMediator mediator,
    ILogger<BiddingOutbidEmailEventConsumerService> logger) : ICapSubscribe
{
    private const string RoutingKey = "bidding.bid.outbid-email.requested";
    public const string QueueName = "mail-service.bidding-outbid-email";

    [CapSubscribe(RoutingKey, Group = QueueName)]
    public async Task HandleAsync(
        BiddingOutbidEmailEvent eventData,
        CancellationToken cancellationToken)
    {
        if (eventData.RecipientUserIds.Count == 0)
        {
            return;
        }

        var recipients = await emailLookupClient.GetEmailsAsync(
            eventData.RecipientUserIds,
            cancellationToken);

        foreach (var recipient in recipients.Where(recipient => !string.IsNullOrWhiteSpace(recipient.Email)))
        {
            await mediator.Send(new SendEmailCommandRequest(
                recipient.Email,
                BiddingOutbidEmailTemplate.BuildSubject(),
                BiddingOutbidEmailTemplate.BuildBody(eventData)
            ));
        }

        logger.LogInformation(
            "Bidding outbid emails processed. AuctionId: {AuctionId}, RecipientCount: {RecipientCount}",
            eventData.AuctionId,
            recipients.Count);
    }
}

using FlashMediator;
using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace Licit.WalletService.API.Payments;

public class StripeWalletDepositService(
    WalletDbContext dbContext,
    IMediator mediator,
    IOptions<StripeWalletOptions> options,
    PaymentIntentService paymentIntentService,
    ILogger<StripeWalletDepositService> logger)
{
    private const string DepositIdMetadataKey = "wallet_deposit_payment_id";
    private const string UserIdMetadataKey = "user_id";

    private readonly StripeWalletOptions _options = options.Value;

    public async Task<WalletDepositPaymentIntentResponse> CreatePaymentIntentAsync(
        Guid userId,
        decimal amount,
        string clientIdempotencyKey,
        CancellationToken cancellationToken)
    {
        EnsureStripeSecretConfigured();
        ValidateAmount(amount);

        var normalizedAmount = decimal.Round(amount, 2);
        var currency = NormalizeCurrency(_options.Currency);
        clientIdempotencyKey = NormalizeIdempotencyKey(clientIdempotencyKey);

        var existingPayment = await dbContext.WalletDepositPayments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                payment => payment.UserId == userId && payment.ClientIdempotencyKey == clientIdempotencyKey,
                cancellationToken);

        if (existingPayment?.StripePaymentIntentId is not null)
        {
            var existingIntent = await paymentIntentService.GetAsync(
                existingPayment.StripePaymentIntentId,
                requestOptions: CreateRequestOptions(),
                cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(existingIntent.ClientSecret))
                throw new ConflictException("Odeme oturumu tekrar kullanilamiyor. Lutfen yeni bir yukleme baslat.");

            return new WalletDepositPaymentIntentResponse(
                existingPayment.Id,
                existingIntent.Id,
                existingIntent.ClientSecret,
                existingPayment.Amount,
                existingPayment.Currency);
        }

        if (existingPayment is not null)
            throw new ConflictException("Bu yukleme istegi henuz hazir degil. Lutfen yeni bir yukleme baslat.");

        var payment = new WalletDepositPayment(userId, normalizedAmount, currency, clientIdempotencyKey);
        dbContext.WalletDepositPayments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var paymentIntent = await paymentIntentService.CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Amount = ToMinorUnits(normalizedAmount),
                    Currency = currency,
                    Description = "Licit cuzdan yukleme",
                    Metadata = new Dictionary<string, string>
                    {
                        [DepositIdMetadataKey] = payment.Id.ToString("D"),
                        [UserIdMetadataKey] = userId.ToString("D")
                    },
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                        AllowRedirects = "never"
                    }
                },
                CreateRequestOptions(clientIdempotencyKey),
                cancellationToken);

            payment.RegisterPaymentIntent(paymentIntent.Id);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new WalletDepositPaymentIntentResponse(
                payment.Id,
                paymentIntent.Id,
                paymentIntent.ClientSecret,
                payment.Amount,
                payment.Currency);
        }
        catch (StripeException exception)
        {
            payment.MarkFailed(exception.StripeError?.Code, exception.StripeError?.Message ?? exception.Message);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogWarning(exception, "Stripe PaymentIntent olusturulamadi. DepositPaymentId: {DepositPaymentId}", payment.Id);
            throw new BusinessRuleException("Odeme baslatilamadi. Lutfen kart bilgilerini kontrol edip tekrar dene.");
        }
    }

    public async Task<WalletDepositPaymentSyncResponse> SyncSucceededPaymentIntentAsync(
        Guid userId,
        string paymentIntentId,
        CancellationToken cancellationToken)
    {
        EnsureStripeSecretConfigured();

        var paymentIntent = await paymentIntentService.GetAsync(
            paymentIntentId,
            requestOptions: CreateRequestOptions(),
            cancellationToken: cancellationToken);

        return await ApplyPaymentIntentAsync(paymentIntent, userId, cancellationToken);
    }

    public async Task HandleStripeWebhookAsync(string payload, string signature, CancellationToken cancellationToken)
    {
        EnsureStripeSecretConfigured();

        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
            throw new InvalidOperationException("Stripe webhook secret is not configured.");

        var stripeEvent = EventUtility.ConstructEvent(
            payload,
            signature,
            _options.WebhookSecret);

        if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
            return;

        switch (stripeEvent.Type)
        {
            case EventTypes.PaymentIntentSucceeded:
                await ApplyPaymentIntentAsync(paymentIntent, expectedUserId: null, cancellationToken);
                break;
            case EventTypes.PaymentIntentPaymentFailed:
                await MarkPaymentFailedAsync(paymentIntent, cancellationToken);
                break;
            case EventTypes.PaymentIntentCanceled:
                await MarkPaymentCanceledAsync(paymentIntent, cancellationToken);
                break;
        }
    }

    private async Task<WalletDepositPaymentSyncResponse> ApplyPaymentIntentAsync(
        PaymentIntent paymentIntent,
        Guid? expectedUserId,
        CancellationToken cancellationToken)
    {
        var payment = await FindPaymentAsync(paymentIntent, cancellationToken)
            ?? throw new BusinessRuleException("Odeme kaydi bulunamadi.");

        if (expectedUserId.HasValue && payment.UserId != expectedUserId.Value)
            throw new UnauthorizedException("Bu odeme kaydina erisim yetkin yok.");

        if (payment.Status == WalletDepositPaymentStatus.Succeeded)
            return new WalletDepositPaymentSyncResponse(
                payment.Id,
                paymentIntent.Id,
                payment.Status.ToString(),
                true,
                payment.WalletTransactionId,
                payment.Amount,
                payment.Currency);

        if (!string.Equals(paymentIntent.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            return new WalletDepositPaymentSyncResponse(
                payment.Id,
                paymentIntent.Id,
                paymentIntent.Status,
                false,
                payment.WalletTransactionId,
                payment.Amount,
                payment.Currency);

        VerifyPaymentIntentMatches(payment, paymentIntent);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var depositResult = await mediator.Send(
            new DepositFundsCommandRequest(
                payment.UserId,
                payment.Amount,
                $"stripe:{paymentIntent.Id}",
                payment.Id,
                "Stripe kart ile cüzdan yükleme"),
            cancellationToken);

        payment.MarkSucceeded(depositResult.TransactionId);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new WalletDepositPaymentSyncResponse(
            payment.Id,
            paymentIntent.Id,
            payment.Status.ToString(),
            true,
            depositResult.TransactionId,
            payment.Amount,
            payment.Currency);
    }

    private async Task MarkPaymentFailedAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken)
    {
        var payment = await FindPaymentAsync(paymentIntent, cancellationToken);
        if (payment is null || payment.Status == WalletDepositPaymentStatus.Succeeded)
            return;

        payment.MarkFailed(paymentIntent.LastPaymentError?.Code, paymentIntent.LastPaymentError?.Message);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task MarkPaymentCanceledAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken)
    {
        var payment = await FindPaymentAsync(paymentIntent, cancellationToken);
        if (payment is null || payment.Status == WalletDepositPaymentStatus.Succeeded)
            return;

        payment.MarkCanceled();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<WalletDepositPayment?> FindPaymentAsync(
        PaymentIntent paymentIntent,
        CancellationToken cancellationToken)
    {
        var payment = await dbContext.WalletDepositPayments
            .FirstOrDefaultAsync(
                depositPayment => depositPayment.StripePaymentIntentId == paymentIntent.Id,
                cancellationToken);

        if (payment is not null)
            return payment;

        if (paymentIntent.Metadata is null ||
            !paymentIntent.Metadata.TryGetValue(DepositIdMetadataKey, out var depositPaymentIdValue) ||
            !Guid.TryParse(depositPaymentIdValue, out var depositPaymentId))
        {
            return null;
        }

        payment = await dbContext.WalletDepositPayments
            .FirstOrDefaultAsync(depositPayment => depositPayment.Id == depositPaymentId, cancellationToken);

        if (payment is not null && payment.StripePaymentIntentId is null)
        {
            payment.RegisterPaymentIntent(paymentIntent.Id);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return payment;
    }

    private void VerifyPaymentIntentMatches(WalletDepositPayment payment, PaymentIntent paymentIntent)
    {
        if (!string.Equals(paymentIntent.Currency, payment.Currency, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Odeme para birimi cuzdan yukleme kaydiyla eslesmiyor.");

        var receivedAmount = paymentIntent.AmountReceived > 0
            ? paymentIntent.AmountReceived
            : paymentIntent.Amount;

        if (receivedAmount != ToMinorUnits(payment.Amount))
            throw new BusinessRuleException("Odeme tutari cuzdan yukleme kaydiyla eslesmiyor.");
    }

    private void ValidateAmount(decimal amount)
    {
        if (amount < _options.MinimumAmount)
            throw new BusinessRuleException($"Yukleme tutari en az {_options.MinimumAmount:0.##} olmalidir.");

        if (amount > _options.MaximumAmount)
            throw new BusinessRuleException($"Yukleme tutari en fazla {_options.MaximumAmount:0.##} olabilir.");

        if (decimal.Round(amount, 2) != amount)
            throw new BusinessRuleException("Yukleme tutari en fazla iki ondalik basamak icerebilir.");
    }

    private void EnsureStripeSecretConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("Stripe secret key is not configured.");
    }

    private RequestOptions CreateRequestOptions(string? idempotencyKey = null) =>
        new()
        {
            ApiKey = _options.SecretKey,
            IdempotencyKey = idempotencyKey
        };

    private static string NormalizeCurrency(string currency) =>
        string.IsNullOrWhiteSpace(currency)
            ? "try"
            : currency.Trim().ToLowerInvariant();

    private static string NormalizeIdempotencyKey(string idempotencyKey) =>
        string.IsNullOrWhiteSpace(idempotencyKey)
            ? Guid.CreateVersion7().ToString("D")
            : idempotencyKey.Trim();

    private static long ToMinorUnits(decimal amount) =>
        decimal.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
}

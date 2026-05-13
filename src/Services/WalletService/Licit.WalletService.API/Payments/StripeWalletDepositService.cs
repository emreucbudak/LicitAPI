using Licit.WalletService.Application.Exceptions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace Licit.WalletService.API.Payments;

public class StripeWalletDepositService(
    WalletDbContext dbContext,
    IWalletProvisioningService walletProvisioningService,
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

        var normalizedAmount = checked((int)amount);
        var currency = NormalizeCurrency(_options.Currency);
        clientIdempotencyKey = NormalizeIdempotencyKey(clientIdempotencyKey);

        logger.LogInformation(
            "Stripe cuzdan yukleme baslatiliyor. UserId: {UserId}, Amount: {Amount}, Currency: {Currency}, ClientIdempotencyKey: {ClientIdempotencyKey}",
            userId,
            normalizedAmount,
            currency,
            clientIdempotencyKey);

        await walletProvisioningService.EnsureWalletExistsAsync(userId, cancellationToken);

        var existingPayment = await dbContext.WalletDepositPayments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                payment => payment.UserId == userId && payment.ClientIdempotencyKey == clientIdempotencyKey,
                cancellationToken);

        if (existingPayment?.StripePaymentIntentId is not null)
        {
            logger.LogInformation(
                "Stripe cuzdan yukleme icin mevcut PaymentIntent kullaniliyor. UserId: {UserId}, DepositPaymentId: {DepositPaymentId}, PaymentIntentId: {PaymentIntentId}, Status: {Status}",
                userId,
                existingPayment.Id,
                existingPayment.StripePaymentIntentId,
                existingPayment.Status);

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

        logger.LogInformation(
            "Stripe cuzdan yukleme kaydi olusturuldu. UserId: {UserId}, DepositPaymentId: {DepositPaymentId}, Amount: {Amount}, Currency: {Currency}",
            userId,
            payment.Id,
            payment.Amount,
            payment.Currency);

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

            logger.LogInformation(
                "Stripe PaymentIntent cuzdan yukleme kaydina baglandi. UserId: {UserId}, DepositPaymentId: {DepositPaymentId}, PaymentIntentId: {PaymentIntentId}, StripeStatus: {StripeStatus}",
                userId,
                payment.Id,
                paymentIntent.Id,
                paymentIntent.Status);

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
        logger.LogInformation(
            "Stripe PaymentIntent apply kontrolu basladi. PaymentIntentId: {PaymentIntentId}, StripeStatus: {StripeStatus}, ExpectedUserId: {ExpectedUserId}",
            paymentIntent.Id,
            paymentIntent.Status,
            expectedUserId);

        var payment = await FindPaymentAsync(paymentIntent, lockForUpdate: false, cancellationToken)
            ?? throw new BusinessRuleException("Odeme kaydi bulunamadi.");

        logger.LogInformation(
            "Stripe odeme kaydi bulundu. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, UserId: {UserId}, Status: {Status}, Amount: {Amount}, WalletTransactionId: {WalletTransactionId}",
            paymentIntent.Id,
            payment.Id,
            payment.UserId,
            payment.Status,
            payment.Amount,
            payment.WalletTransactionId);

        if (expectedUserId.HasValue && payment.UserId != expectedUserId.Value)
            throw new UnauthorizedException("Bu odeme kaydina erisim yetkin yok.");

        if (IsSucceeded(payment))
        {
            logger.LogInformation(
                "Stripe odeme daha once wallet bakiyesine uygulanmis. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, WalletTransactionId: {WalletTransactionId}",
                paymentIntent.Id,
                payment.Id,
                payment.WalletTransactionId);

            return new WalletDepositPaymentSyncResponse(
                payment.Id,
                paymentIntent.Id,
                payment.Status.ToString(),
                true,
                payment.WalletTransactionId,
                payment.Amount,
                payment.Currency);
        }

        if (!string.Equals(paymentIntent.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation(
                "Stripe odeme henuz succeeded degil. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, StripeStatus: {StripeStatus}",
                paymentIntent.Id,
                payment.Id,
                paymentIntent.Status);

            return new WalletDepositPaymentSyncResponse(
                payment.Id,
                paymentIntent.Id,
                paymentIntent.Status,
                false,
                payment.WalletTransactionId,
                payment.Amount,
                payment.Currency);
        }

        await walletProvisioningService.EnsureWalletExistsAsync(payment.UserId, cancellationToken);

        dbContext.Entry(payment).State = EntityState.Detached;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            payment = await FindPaymentAsync(paymentIntent, lockForUpdate: true, cancellationToken)
                ?? throw new BusinessRuleException("Odeme kaydi bulunamadi.");

            logger.LogInformation(
                "Stripe odeme kaydi kilitli olarak alindi. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, UserId: {UserId}, Status: {Status}",
                paymentIntent.Id,
                payment.Id,
                payment.UserId,
                payment.Status);

            if (expectedUserId.HasValue && payment.UserId != expectedUserId.Value)
                throw new UnauthorizedException("Bu odeme kaydina erisim yetkin yok.");

            if (IsSucceeded(payment))
            {
                await transaction.CommitAsync(cancellationToken);
                return new WalletDepositPaymentSyncResponse(
                    payment.Id,
                    paymentIntent.Id,
                    payment.Status.ToString(),
                    true,
                    payment.WalletTransactionId,
                    payment.Amount,
                    payment.Currency);
            }

            var stripeAmount = VerifyPaymentIntentMatches(payment, paymentIntent);

            var depositTransaction = await ApplyWalletDepositAsync(
                payment,
                stripeAmount,
                paymentIntent.Id,
                cancellationToken);

            payment.MarkSucceeded(depositTransaction.Id);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                "Stripe cuzdan yukleme tamamlandi. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, UserId: {UserId}, TransactionId: {TransactionId}, Amount: {Amount}, Status: {Status}",
                paymentIntent.Id,
                payment.Id,
                payment.UserId,
                depositTransaction.Id,
                stripeAmount,
                payment.Status);

            return new WalletDepositPaymentSyncResponse(
                payment.Id,
                paymentIntent.Id,
                payment.Status.ToString(),
                true,
                depositTransaction.Id,
                stripeAmount,
                payment.Currency);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogError(
                exception,
                "Stripe cuzdan yukleme concurrency hatasi. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, UserId: {UserId}, Status: {PaymentStatus}, Entries: {Entries}",
                paymentIntent.Id,
                payment.Id,
                payment.UserId,
                payment.Status,
                string.Join(", ", exception.Entries.Select(entry => $"{entry.Entity.GetType().Name}:{entry.State}")));

            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<WalletTransaction> ApplyWalletDepositAsync(
        WalletDepositPayment payment,
        int amount,
        string paymentIntentId,
        CancellationToken cancellationToken)
    {
        var wallet = await dbContext.Wallets
            .FirstOrDefaultAsync(existingWallet => existingWallet.UserId == payment.UserId, cancellationToken)
            ?? throw new BusinessRuleException("Cuzdan bulunamadi.");

        var existingTransaction = await dbContext.WalletTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                transaction => transaction.WalletId == wallet.Id &&
                               transaction.Type == TransactionType.Deposit &&
                               transaction.ReferenceId == payment.Id,
                cancellationToken);

        if (existingTransaction is not null)
        {
            logger.LogInformation(
                "Stripe cuzdan yukleme daha once uygulanmis. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, UserId: {UserId}, WalletId: {WalletId}, TransactionId: {TransactionId}",
                paymentIntentId,
                payment.Id,
                payment.UserId,
                wallet.Id,
                existingTransaction.Id);

            return existingTransaction;
        }

        logger.LogInformation(
            "Stripe cuzdan yukleme uygulanacak. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, UserId: {UserId}, WalletId: {WalletId}, Amount: {Amount}, BalanceBefore: {BalanceBefore}, FrozenBefore: {FrozenBefore}",
            paymentIntentId,
            payment.Id,
            payment.UserId,
            wallet.Id,
            amount,
            wallet.Balance,
            wallet.FrozenBalance);

        var transaction = wallet.Deposit(amount, payment.Id, "Cüzdan Bakiye yükleme");
        dbContext.WalletTransactions.Add(transaction);

        logger.LogInformation(
            "Stripe cuzdan yukleme entity uzerinde uygulandi. PaymentIntentId: {PaymentIntentId}, DepositPaymentId: {DepositPaymentId}, UserId: {UserId}, WalletId: {WalletId}, TransactionId: {TransactionId}, BalanceAfter: {BalanceAfter}, FrozenAfter: {FrozenAfter}",
            paymentIntentId,
            payment.Id,
            payment.UserId,
            wallet.Id,
            transaction.Id,
            wallet.Balance,
            wallet.FrozenBalance);

        return transaction;
    }

    private async Task MarkPaymentFailedAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var payment = await FindPaymentAsync(paymentIntent, lockForUpdate: true, cancellationToken);
        if (payment is null || IsSucceeded(payment))
            return;

        payment.MarkFailed(paymentIntent.LastPaymentError?.Code, paymentIntent.LastPaymentError?.Message);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task MarkPaymentCanceledAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var payment = await FindPaymentAsync(paymentIntent, lockForUpdate: true, cancellationToken);
        if (payment is null || IsSucceeded(payment))
            return;

        payment.MarkCanceled();
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<WalletDepositPayment?> FindPaymentAsync(
        PaymentIntent paymentIntent,
        bool lockForUpdate,
        CancellationToken cancellationToken)
    {
        var payment = lockForUpdate
            ? await dbContext.WalletDepositPayments
                .FromSqlInterpolated($"""
                    SELECT * FROM "WalletDepositPayments"
                    WHERE "StripePaymentIntentId" = {paymentIntent.Id}
                    FOR UPDATE
                    """)
                .FirstOrDefaultAsync(cancellationToken)
            : await dbContext.WalletDepositPayments
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

        payment = lockForUpdate
            ? await dbContext.WalletDepositPayments
                .FromSqlInterpolated($"""
                    SELECT * FROM "WalletDepositPayments"
                    WHERE "Id" = {depositPaymentId}
                    FOR UPDATE
                    """)
                .FirstOrDefaultAsync(cancellationToken)
            : await dbContext.WalletDepositPayments
                .FirstOrDefaultAsync(depositPayment => depositPayment.Id == depositPaymentId, cancellationToken);

        if (payment?.StripePaymentIntentId is not null &&
            !string.Equals(payment.StripePaymentIntentId, paymentIntent.Id, StringComparison.Ordinal))
        {
            return null;
        }

        if (payment is not null && payment.StripePaymentIntentId is null)
        {
            payment.RegisterPaymentIntent(paymentIntent.Id);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return payment;
    }

    private int VerifyPaymentIntentMatches(WalletDepositPayment payment, PaymentIntent paymentIntent)
    {
        if (!string.Equals(paymentIntent.Currency, payment.Currency, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Odeme para birimi cuzdan yukleme kaydiyla eslesmiyor.");

        var receivedAmount = paymentIntent.AmountReceived > 0
            ? paymentIntent.AmountReceived
            : paymentIntent.Amount;

        if (receivedAmount != ToMinorUnits(payment.Amount))
            throw new BusinessRuleException("Odeme tutari cuzdan yukleme kaydiyla eslesmiyor.");

        return payment.Amount;
    }

    private void ValidateAmount(decimal amount)
    {
        if (amount < _options.MinimumAmount)
            throw new BusinessRuleException($"Yukleme tutari en az {_options.MinimumAmount:0.##} olmalidir.");

        if (amount > _options.MaximumAmount)
            throw new BusinessRuleException($"Yukleme tutari en fazla {_options.MaximumAmount:0.##} olabilir.");

        if (decimal.Truncate(amount) != amount)
            throw new BusinessRuleException("Yukleme tutari tam TL olmalidir.");
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

    private static bool IsSucceeded(WalletDepositPayment payment) =>
        Convert.ToInt32(payment.Status) == 1;
}

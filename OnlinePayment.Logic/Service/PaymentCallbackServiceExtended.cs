using OnlinePayment.Logic.DataAccess;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentCallbackServiceExtended : IPaymentCallbackService
    {
        Task<PaymentCallback> Insert(PaymentCallback model, string externalId);
    }

    public partial class PaymentCallbackServiceExtended : PaymentCallbackService, IPaymentCallbackServiceExtended
    {
        private const string PaidStatus = "PAID";

        private readonly IPaymentServiceExtended paymentService;
        private readonly IAuditServiceExtended auditService;
        private readonly IKohaService kohaService;
        private readonly SwishApiSettings swishApiSettings;

        public PaymentCallbackServiceExtended(ILogger<PaymentCallbackService> logger,
           IPaymentCallbackDataAccess dataAccess,
           IPaymentServiceExtended paymentService,
           IAuditServiceExtended auditService,
           IKohaService kohaService,
           IOptions<SwishApiSettings> swishApiOptions)
           : base(logger, dataAccess)
        {
            this.paymentService = paymentService;
            this.auditService = auditService;
            this.kohaService = kohaService;
            this.swishApiSettings = swishApiOptions.Value;
        }

        public async Task<PaymentCallback> Insert(PaymentCallback model, string externalId)
        {
            // The callback endpoint is unauthenticated (the payment provider posts to it),
            // so no value in the request body can be trusted. GetByExternalId has just
            // refreshed payment.Status from the provider's own API — that is the only
            // authoritative source for whether the payment actually went through.
            var payment = await paymentService.GetByExternalId(externalId);
            if (payment == null) throw new Exception($"No payment with external id {externalId}");
            model.Session = payment.Session;

            // Only credit when the provider itself reports the payment as paid. Never rely
            // on the callback's own DatePaid/Status/Amount.
            if (!string.Equals(payment.Status, PaidStatus, StringComparison.OrdinalIgnoreCase))
            {
                var notPaidMessage = $"Callback for session {payment.Session} ignored: provider status is '{payment.Status}', not {PaidStatus}";
                logger.LogInformation(notPaidMessage);
                await auditService.AddAudit(notPaidMessage, model.Session, typeof(PaymentCallback));
                return null;
            }

            // Verify the callback's currency matches the currency the payment was created
            // with. Only reject on a definite mismatch (a provided value that differs), so a
            // callback that omits the field is not falsely rejected.
            var expectedCurrency = swishApiSettings.Currency;
            if (!string.IsNullOrEmpty(model.Currency)
                && !string.IsNullOrEmpty(expectedCurrency)
                && !string.Equals(model.Currency, expectedCurrency, StringComparison.OrdinalIgnoreCase))
            {
                var currencyMismatch = $"Callback for session {payment.Session} ignored: currency '{model.Currency}' does not match expected '{expectedCurrency}'";
                logger.LogWarning(currencyMismatch);
                await auditService.AddAudit(currencyMismatch, model.Session, typeof(PaymentCallback));
                return null;
            }

            // Idempotency: a payment must be credited at most once, even if the callback is
            // delivered or replayed several times. A persisted callback for this session
            // means it has already been processed. (For strict protection against truly
            // concurrent duplicates, enforce a unique constraint on the callback session in
            // the database.)
            var alreadyProcessed = (await GetAll()).Any(c => c.Session == model.Session);
            if (alreadyProcessed)
            {
                var duplicateMessage = $"Duplicate callback for session {model.Session} ignored; payment already credited";
                logger.LogInformation(duplicateMessage);
                await auditService.AddAudit(duplicateMessage, model.Session, typeof(PaymentCallback));
                return null;
            }

            // Defence in depth: credit the stored, server-side amount — never the amount
            // supplied in the callback body. A mismatch indicates a tampered callback.
            var callbackAmount = Utils.ConvertToInt(model.Amount);
            if (callbackAmount != payment.Amount)
            {
                await auditService.AddAudit($"Callback amount {callbackAmount} differs from stored amount {payment.Amount}; crediting stored amount", model.Session, typeof(PaymentCallback));
            }

            var newModel = await base.Insert(model);
            await auditService.AddAudit("Payment callback saved", model.Session, typeof(PaymentCallback));
            await kohaService.UpdateSum(payment.BorrowerNumber, payment.Amount, payment.Session);
            return newModel;
        }
    }
}

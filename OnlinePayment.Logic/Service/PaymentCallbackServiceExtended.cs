using OnlinePayment.Logic.DataAccess;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OnlinePayment.Logic.Model;
using System;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentCallbackServiceExtended : IPaymentCallbackService
    {
        Task<PaymentCallback> Insert(PaymentCallback model, string externalId);
    }

    public partial class PaymentCallbackServiceExtended : PaymentCallbackService, IPaymentCallbackServiceExtended
    {
        private readonly IPaymentServiceExtended paymentService;
        private readonly IAuditServiceExtended auditService;
        private readonly IKohaService kohaService;

        public PaymentCallbackServiceExtended(ILogger<PaymentCallbackService> logger,
           IPaymentCallbackDataAccess dataAccess,
           IPaymentServiceExtended paymentService,
           IAuditServiceExtended auditService,
           IKohaService kohaService)
           : base(logger, dataAccess)
        {
            this.paymentService = paymentService;
            this.auditService = auditService;
            this.kohaService = kohaService;
        }

        public async Task<PaymentCallback> Insert(PaymentCallback model, string externalId)
        {
            var payment = await paymentService.GetByExternalId(externalId);
            if (payment == null) throw new Exception($"No payment with external id {externalId}");
            model.Session = payment.Session;
            if (model.DatePaid == null)
            {
                var noPaymentMessage = $"No payment registrerd for borrower {payment.BorrowerNumber}";
                logger.LogInformation(noPaymentMessage);
                await auditService.AddAudit(noPaymentMessage, model.Session, typeof(PaymentCallback));
                return null;
            }
            var newModel = await base.Insert(model);
            await auditService.AddAudit("Payment callback saved", model.Session, typeof(PaymentCallback));
            await kohaService.UpdateSum(payment.BorrowerNumber, Utils.ConvertToInt(model.Amount), payment.Session);
            return newModel;
        }
    }
}

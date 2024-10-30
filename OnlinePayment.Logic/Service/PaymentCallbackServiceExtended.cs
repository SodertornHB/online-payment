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
        private readonly IAuditService auditService;
        private readonly IKohaService kohaService;

        public PaymentCallbackServiceExtended(ILogger<PaymentCallbackService> logger,
           IPaymentCallbackDataAccess dataAccess,
           IPaymentServiceExtended paymentService,
           IAuditService auditService,
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
            var newModel = await base.Insert(model);
            await auditService.Insert(new Audit("Payment callback saved", model.Session, typeof(PaymentCallback)));
            await kohaService.UpdateSum(payment.BorrowerNumber, Utils.ConvertToInt(model.Amount), payment.Session);
            return newModel;
        }
    }
}

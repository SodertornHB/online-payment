using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OnlinePayment.Logic.Services
{
    public partial class PaymentResponseServiceExtended : PaymentResponseService
    {
        private readonly IAuditService auditService;

        public PaymentResponseServiceExtended(ILogger<PaymentResponseService> logger,
           IPaymentResponseDataAccess dataAccess,
           IAuditService auditService)
           : base(logger, dataAccess)
        {
            this.auditService = auditService;
        }

        public override async Task<PaymentResponse> Insert(PaymentResponse model)
        {
            var paymentResponse = await base.Insert(model);

            await auditService.Insert(new Audit("Payment response saved", model.Session, typeof(PaymentResponse)));

            return paymentResponse;
        }
    }
}

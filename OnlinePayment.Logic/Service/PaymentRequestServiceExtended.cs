using Microsoft.Extensions.Logging;
using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using System;
using System.Threading.Tasks;

namespace OnlinePayment.Logic.Services
{

    public partial class PaymentRequestServiceExtended : PaymentRequestService
    {
        public PaymentRequestServiceExtended(ILogger<PaymentRequestService> logger,
           IPaymentRequestDataAccess dataAccess)
           : base(logger, dataAccess)
        { }
        public override Task<PaymentRequest> Insert(PaymentRequest model)
        {
            if (!model.IsValid())
            {
                throw new FormatException("Amount must be a valid decimal number.");
            }

            model.ConvertAmountToInt();

            return base.Insert(model);
        }

    }
}

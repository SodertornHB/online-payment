
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using Microsoft.Extensions.Logging;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentRequestService : IService<PaymentRequest>
    {
    }

    public partial class PaymentRequestService : Service<PaymentRequest>, IPaymentRequestService
    {
        public PaymentRequestService(ILogger<PaymentRequestService> logger,
           IPaymentRequestDataAccess dataAccess)
           : base(logger, dataAccess)
        { }
    }
}

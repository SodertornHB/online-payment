
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using Microsoft.Extensions.Logging;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentResponseService : IService<PaymentResponse>
    {
    }

    public partial class PaymentResponseService : Service<PaymentResponse>, IPaymentResponseService
    {
        public PaymentResponseService(ILogger<PaymentResponseService> logger,
           IPaymentResponseDataAccess dataAccess)
           : base(logger, dataAccess)
        { }
    }
}

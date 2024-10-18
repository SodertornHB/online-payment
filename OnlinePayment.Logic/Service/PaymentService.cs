
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using Microsoft.Extensions.Logging;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentService : IService<Payment>
    {
    }

    public partial class PaymentService : Service<Payment>, IPaymentService
    {
        public PaymentService(ILogger<PaymentService> logger,
           IPaymentDataAccess dataAccess)
           : base(logger, dataAccess)
        { }
    }
}

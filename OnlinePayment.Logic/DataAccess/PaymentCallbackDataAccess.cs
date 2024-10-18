
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.DataAccess;

namespace OnlinePayment.Logic.DataAccess
{
    public interface IPaymentCallbackDataAccess : IDataAccess<PaymentCallback>
    {    }

    public class PaymentCallbackDataAccess : BaseDataAccess<PaymentCallback>, IPaymentCallbackDataAccess
    {
        public PaymentCallbackDataAccess(ISqlDataAccess db, SqlStringBuilder<PaymentCallback> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 
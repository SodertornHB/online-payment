
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.DataAccess;

namespace OnlinePayment.Logic.DataAccess
{
    public interface IPaymentResponseDataAccess : IDataAccess<PaymentResponse>
    {    }

    public class PaymentResponseDataAccess : BaseDataAccess<PaymentResponse>, IPaymentResponseDataAccess
    {
        public PaymentResponseDataAccess(ISqlDataAccess db, SqlStringBuilder<PaymentResponse> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 
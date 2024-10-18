
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.DataAccess;

namespace OnlinePayment.Logic.DataAccess
{
    public interface IPaymentRequestDataAccess : IDataAccess<PaymentRequest>
    {    }

    public class PaymentRequestDataAccess : BaseDataAccess<PaymentRequest>, IPaymentRequestDataAccess
    {
        public PaymentRequestDataAccess(ISqlDataAccess db, SqlStringBuilder<PaymentRequest> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 
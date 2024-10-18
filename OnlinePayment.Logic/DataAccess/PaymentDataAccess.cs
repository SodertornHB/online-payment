
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.DataAccess;

namespace OnlinePayment.Logic.DataAccess
{
    public interface IPaymentDataAccess : IDataAccess<Payment>
    {    }

    public class PaymentDataAccess : BaseDataAccess<Payment>, IPaymentDataAccess
    {
        public PaymentDataAccess(ISqlDataAccess db, SqlStringBuilder<Payment> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 
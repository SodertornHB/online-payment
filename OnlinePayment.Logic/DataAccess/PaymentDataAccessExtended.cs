using OnlinePayment.Logic.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlinePayment.Logic.DataAccess
{
    public interface IPaymentDataAccessExtended : IPaymentDataAccess 
    {
        Task<IEnumerable<Payment>> GetUnpaid();
    }

    public class PaymentDataAccessExtended : PaymentDataAccess, IPaymentDataAccessExtended
    {
        public PaymentDataAccessExtended(ISqlDataAccess db, SqlStringBuilder<Payment> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }

        public virtual async Task<IEnumerable<Payment>> GetUnpaid()
        {
            string sql = $"SELECT * FROM [{Table}] where status <> 'PAID'";
            return await ExecuteSelectMany(sql);
        }
    }
} 
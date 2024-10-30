using OnlinePayment.Logic.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlinePayment.Logic.DataAccess
{
    public interface IPaymentDataAccessExtended : IPaymentDataAccess
    {
        Task<Payment> GetByExternalId(string externalId);
        Task<Payment> GetBySessionId(string sessionId);
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
        public virtual async Task<Payment> GetByExternalId(string externalId)
        {
            string sql = $"SELECT * FROM [{Table}] where ExternalId like '{externalId}'";
            return await ExecuteSelectSingle(sql);
        }
        public virtual async Task<Payment> GetBySessionId(string sessionId)
        {
            string sql = $"SELECT * FROM [{Table}] where session like '{sessionId}'";
            return await ExecuteSelectSingle(sql);
        }
    }
} 

//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.DataAccess;

namespace OnlinePayment.Logic.DataAccess
{
    public interface IAuditDataAccess : IDataAccess<Audit>
    {    }

    public class AuditDataAccess : BaseDataAccess<Audit>, IAuditDataAccess
    {
        public AuditDataAccess(ISqlDataAccess db, SqlStringBuilder<Audit> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 
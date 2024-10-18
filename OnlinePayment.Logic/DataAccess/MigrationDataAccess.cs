
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.DataAccess;

namespace OnlinePayment.Logic.DataAccess
{
    public interface IMigrationDataAccess : IDataAccess<Migration>
    {    }

    public class MigrationDataAccess : BaseDataAccess<Migration>, IMigrationDataAccess
    {
        public MigrationDataAccess(ISqlDataAccess db, SqlStringBuilder<Migration> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 
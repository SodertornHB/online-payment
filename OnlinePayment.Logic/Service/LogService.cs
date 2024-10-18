
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using Microsoft.Extensions.Logging;

namespace OnlinePayment.Logic.Services
{
    public partial interface ILogService : IService<Log>
    {
    }

    public partial class LogService : Service<Log>, ILogService
    {
        public LogService(ILogger<LogService> logger,
           ILogDataAccess dataAccess)
           : base(logger, dataAccess)
        { }
    }
}

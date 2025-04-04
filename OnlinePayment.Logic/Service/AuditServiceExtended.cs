using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace OnlinePayment.Logic.Services
{
    public partial interface IAuditServiceExtended : IAuditService
    {
        Task AddAudit(string message, string session, Type type);
    }

    public partial class AuditServiceExtended : AuditService, IAuditServiceExtended
    {
        public AuditServiceExtended(ILogger<AuditService> logger,
           IAuditDataAccess dataAccess)
           : base(logger, dataAccess)
        { }

        public async Task AddAudit(string message, string session, Type type)
        {
            var audit = new Audit(message, session, type);
            await base.Insert(audit);
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Http;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Settings;
using System.Threading.Tasks;

namespace OnlinePayment.Logic.Services
{
    public partial interface IKohaService
    {
        Task<Patron> GetPatron(int biblionumber, string session);
    }

    public partial class KohaService : IKohaService
    {
        private readonly IKohaHttpService kohaHttpService;
        private readonly IAuditService auditService;
        private readonly KohaApiSettings kohaApiSettings;

        public KohaService(ILogger<KohaService> logger,
            IKohaHttpService kohaHttpService,
           IOptions<KohaApiSettings> options,
           IAuditService auditService)
        {
            this.kohaHttpService = kohaHttpService;
            this.auditService = auditService;
            kohaApiSettings = options.Value;
        }

        public async Task<Patron> GetPatron(int biblionumber, string session)
        {
            await auditService.Insert(new Audit("Fetching patron info from Koha", session, typeof(Patron)));
            var patron = await kohaHttpService.Get($"{kohaApiSettings.Endpoint}/patrons", biblionumber.ToString());
            return patron;
        }
    }
}

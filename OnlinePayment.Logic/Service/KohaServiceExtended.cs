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
        Task<PatronAccount> GetAccount(int biblionumber, string session);
        Task UpdateSum(int biblionumber, int amount, string session);
    }

    public partial class KohaService : IKohaService
    {
        private readonly IPatronHttpService kohaHttpService;
        private readonly IPatronAccountHttpService patronAccountHttpService;
        private readonly IAuditService auditService;
        private readonly IPatronCreditHttpService patronCreditHttpService;
        private readonly KohaApiSettings kohaApiSettings;

        public KohaService(ILogger<KohaService> logger,
            IPatronHttpService kohaHttpService,
            IPatronAccountHttpService patronAccountHttpService,
           IOptions<KohaApiSettings> options,
           IAuditService auditService,
           IPatronCreditHttpService patronCreditHttpService)
        {
            this.kohaHttpService = kohaHttpService;
            this.patronAccountHttpService = patronAccountHttpService;
            this.auditService = auditService;
            this.patronCreditHttpService = patronCreditHttpService;
            kohaApiSettings = options.Value;
        }

        public async Task<Patron> GetPatron(int biblionumber, string session)
        {
            await auditService.Insert(new Audit("Fetching patron info from Koha", session, typeof(Patron)));
            var patron = await kohaHttpService.Get($"{kohaApiSettings.Endpoint}/patrons", biblionumber.ToString());
            return patron;
        }
        public async Task<PatronAccount> GetAccount(int biblionumber, string session)
        {
            await auditService.Insert(new Audit("Fetching patron account info from Koha", session, typeof(Patron)));
            var account = await patronAccountHttpService.Get($"{kohaApiSettings.Endpoint}/patrons", $"{biblionumber}/account");
            return account;
        }

        public async Task UpdateSum(int biblionumber, int amount, string session)
        {
            var putModel = new PatronCredit { amount = amount, library_id = kohaApiSettings.LibraryId };
            await patronCreditHttpService.Post($"{kohaApiSettings.Endpoint}/patrons/{biblionumber}/account/credits", putModel);
            await auditService.Insert(new Audit($"Subtracted account with {amount} SEK", session, typeof(Patron)));
        }
    }
}

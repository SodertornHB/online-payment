using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Http;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Settings;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OnlinePayment.Logic.Services
{
    public partial interface IKohaService
    {
        Task<Patron> GetPatron(int biblionumber, string session = "");
        Task<PatronAccount> GetAccount(int biblionumber, string session = "");
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

        public async Task<Patron> GetPatron(int biblionumber, string session = "")
        {
            try
            {
                await AddAudit(session, "Fetching patron info from Koha");
                var patron = await kohaHttpService.Get($"{kohaApiSettings.Endpoint}/patrons", biblionumber.ToString());
                return patron;
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound) throw new ArgumentException($"Patron not found", e);
                throw;
            }
        }

        public async Task<PatronAccount> GetAccount(int biblionumber, string session = "")
        {
            try
            {
                await AddAudit(session, "Fetching patron account info from Koha");
                var account = await patronAccountHttpService.Get($"{kohaApiSettings.Endpoint}/patrons", $"{biblionumber}/account");
                return account;
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound) throw new ArgumentException($"Account not found", e);
                throw;
            }
        }

        public async Task UpdateSum(int biblionumber, int amount, string session)
        {
            var putModel = new PatronCredit { amount = amount, library_id = kohaApiSettings.LibraryId };
            await patronCreditHttpService.Post($"{kohaApiSettings.Endpoint}/patrons/{biblionumber}/account/credits", putModel);
            await AddAudit(session, $"Subtracted account with {amount} SEK");
        }

        private async Task AddAudit(string session, string message)
        {
            if (!string.IsNullOrEmpty(session)) await auditService.Insert(new Audit(message, session, typeof(Patron)));
        }
    }
}

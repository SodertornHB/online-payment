using Microsoft.Extensions.Logging;
using OnlinePayment.Logic.Model;

namespace OnlinePayment.Logic.Http
{
    public interface IPatronCreditHttpService : IHttpService<PatronCredit>
    {
    }
    public class PatronCreditHttpService : HttpService<PatronCredit>, IPatronCreditHttpService
    {

        public PatronCreditHttpService(IKohaHttpClient client,
            ILogger<PatronCreditHttpService> logger)
            : base(client, logger)
        {
        }
    }
}

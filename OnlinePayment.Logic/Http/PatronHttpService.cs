using Microsoft.Extensions.Logging;
using OnlinePayment.Logic.Model;

namespace OnlinePayment.Logic.Http
{
    public interface IPatronHttpService : IHttpService<Patron>
    {
    }
    public class PatronHttpService : HttpService<Patron>, IPatronHttpService
    {

        public PatronHttpService(IKohaHttpClient client,
            ILogger<PatronHttpService> logger)
            : base(client, logger)
        {
        }      

    }
}

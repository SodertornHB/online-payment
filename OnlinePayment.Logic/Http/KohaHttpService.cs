using Microsoft.Extensions.Logging;
using OnlinePayment.Logic.Model;

namespace OnlinePayment.Logic.Http
{
    public interface IKohaHttpService : IHttpService<Patron>
    {
    }
    public class KohaHttpService : HttpService<Patron>, IKohaHttpService
    {

        public KohaHttpService(IKohaHttpClient client,
            ILogger<KohaHttpService> logger)
            : base(client, logger)
        {
        }      

    }
}

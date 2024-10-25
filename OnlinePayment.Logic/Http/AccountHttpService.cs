using Microsoft.Extensions.Logging;
using OnlinePayment.Logic.Model;

namespace OnlinePayment.Logic.Http
{
    public interface IPatronAccountHttpService : IHttpService<PatronAccount>
    {
    }
    public class PatronAccountHttpService : HttpService<PatronAccount>, IPatronAccountHttpService
    {

        public PatronAccountHttpService(IKohaHttpClient client,
            ILogger<PatronAccountHttpService> logger)
            : base(client, logger)
        {
        }      

    }
}

using OnlinePayment.Logic.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OnlinePayment.Logic.Http
{
    public interface IKohaHttpClient : IHttpClient
    { }

    public class KohaHttpClient : HttpClient, IKohaHttpClient
    {
        private readonly KohaApiSettings apiSettings;

        public KohaHttpClient(IHttpClientFactory clientFactory,
            ILogger<HttpClient> logger,
            IOptions<AuthenticationSettings> settings,
            IOptions<KohaApiSettings> options)
            :base(clientFactory, logger, settings)
        {
            apiSettings = options.Value;
        }

        protected override async Task<HttpResponseMessage> Send(HttpRequestMessage request)
        {
            var client = clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiSettings.UserName}:{apiSettings.Password}")));
            return await client.SendAsync(request);        
        }
    }
}

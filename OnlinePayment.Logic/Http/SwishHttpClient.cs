using OnlinePayment.Logic.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace OnlinePayment.Logic.Http
{
    public interface ISwishHttpClient : IHttpClient
    { }

    public class SwishHttpClient : HttpClient, ISwishHttpClient
    {

        protected new CertificationAuthenticationSettings settings;

        public SwishHttpClient(IHttpClientFactory clientFactory,
            ILogger<HttpClient> logger,
            IOptions<CertificationAuthenticationSettings> settings)
            : base(clientFactory, logger, settings)
        {
            this.settings = settings.Value;
        }

        protected override async Task<HttpResponseMessage> Send(HttpRequestMessage request)
        {
            HttpClientHandler handler = GetHttpClientHandlerWithCertificate();

            using (var httpClient = new System.Net.Http.HttpClient(handler))
            {
                if (request.RequestUri != null)
                {
                    httpClient.BaseAddress = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority));
                }

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) return response;
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    string message = $"Error sending request: StatusCode: {response.StatusCode}, Content: {responseContent}";
                    throw new HttpRequestException(message, null, response.StatusCode);
                }
            }
        }

        private HttpClientHandler GetHttpClientHandlerWithCertificate()
        {
            var handler = new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            };

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);

                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, settings.Thumbprint, false);
                handler.ClientCertificates.AddRange(certs);
            }
            return handler;
        }
    }
}

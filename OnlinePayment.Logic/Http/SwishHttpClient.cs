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
            try
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
                        logger.LogInformation(message);
                        throw new HttpRequestException(message, null, response.StatusCode);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error sending request in swish http client");
                throw;
            }
        }

        private HttpClientHandler GetHttpClientHandlerWithCertificate()
        {
            var handler = new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            };

            var certs = new X509Certificate2Collection();
            certs.Import(settings.Certification, settings.Passphrase, X509KeyStorageFlags.DefaultKeySet);

            foreach (var cert in certs)
            {
                try
                {
                    if (cert.HasPrivateKey) handler.ClientCertificates.Add(cert);

                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error with certificate: {cert.Subject}. Error: {e.Message}");
                }
            }
            return handler;
        }
    }
}

using OnlinePayment.Logic.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Linq;

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
            logger.LogInformation($"Sending request using Swish http client");
            HttpClientHandler handler = GetHttpClientHandlerWithCertificate();

            using (var httpClient = new System.Net.Http.HttpClient(handler))
            {
                try
                {
                    if (request.RequestUri != null)
                    {
                        httpClient.BaseAddress = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority));
                    }

                    logger.LogInformation($"Sending request to uri {request.RequestUri}");

                    var response = await httpClient.SendAsync(request);
                    logger.LogInformation($"Response status code {response.StatusCode}");
                    if (response.IsSuccessStatusCode) return response;
                    else
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        string message = $"Error sending request: StatusCode: {response.StatusCode}, Content: {responseContent}";
                        throw new HttpRequestException(message, null, response.StatusCode);
                    }
                }
                catch (Exception e)
                {
                    LogException(e);
                    throw;
                }
            }
        }

        private void LogException(Exception exception)
        {
            if (exception == null) return;

            logger.LogError("Exception Type: {Type}", exception.GetType().Name);
            logger.LogError("Message: {Message}", exception.Message);
            logger.LogError("Stack Trace: {StackTrace}", exception.StackTrace);

            if (exception.Data != null && exception.Data.Count > 0)
            {
                logger.LogError("Exception Data:");
                foreach (var key in exception.Data.Keys)
                {
                    logger.LogError("  {Key}: {Value}", key, exception.Data[key]);
                }
            }

            if (exception.InnerException != null)
            {
                logger.LogError("Inner Exception:");
                LogException(exception.InnerException);
            }
        }

        private HttpClientHandler GetHttpClientHandlerWithCertificate()
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            };

            if (string.IsNullOrWhiteSpace(settings.Thumbprint))
            {
                logger.LogError("Certificate thumbprint is not configured.");
                throw new InvalidOperationException("Certificate thumbprint is required but not provided.");
            }

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                try
                {
                    store.Open(OpenFlags.ReadOnly);

                    logger.LogInformation($"Searching for certificate with thumbprint ending in: {settings.Thumbprint[^6..]}");

                    var certs = store.Certificates.Find(X509FindType.FindByThumbprint, settings.Thumbprint, false);
                    if (certs.Count == 0) throw new InvalidOperationException($"No certificate found with given thumbprint");

                    var selectedCert = certs.OfType<X509Certificate2>().FirstOrDefault();
                    if (selectedCert == null || !selectedCert.HasPrivateKey) throw new InvalidOperationException($"Certificate does not have a private key.");

                    logger.LogInformation($"Certificate found: {selectedCert.Subject}");
                    handler.ClientCertificates.Add(selectedCert);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to configure HttpClientHandler with client certificate.");
                    throw; 
                }
            }

            logger.LogInformation($"Handler contains {handler.ClientCertificates.Count} client certificate(s)");
            return handler;
        }

    }
}

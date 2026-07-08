using OnlinePayment.Logic.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net.Security;
using System.Linq;
using System.IO;

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
            var handler = GetHttpClientHandlerWithCertificate();

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

        private HttpMessageHandler GetHttpClientHandlerWithCertificate()
        {
            var (leaf, intermediates) = LoadClientCertificateWithChain();

            // Use SocketsHttpHandler + SslStreamCertificateContext so the FULL client
            // certificate chain (leaf + issuing CA) is presented during the TLS handshake.
            // HttpClientHandler.ClientCertificates only sends the leaf, which Swish MSS
            // rejects with an "sslv3 alert handshake failure" — curl succeeds because it
            // sends the chain from the PEM. Allow TLS 1.2/1.3 (Swish requires >= 1.2).
            var handler = new SocketsHttpHandler
            {
                SslOptions = new SslClientAuthenticationOptions
                {
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    ClientCertificateContext = SslStreamCertificateContext.Create(leaf, intermediates, offline: true)
                }
            };

            logger.LogInformation($"Handler configured with client certificate {leaf.Subject} and {intermediates.Count} intermediate cert(s)");
            return handler;
        }

        private (X509Certificate2 leaf, X509Certificate2Collection intermediates) LoadClientCertificateWithChain()
        {
            // Prefer loading the PKCS#12 file directly. This is cross-platform and works on
            // Linux, where the Windows certificate store (LocalMachine\My) is not available.
            // Fall back to the certificate store by thumbprint for Windows/IIS hosting.
            if (!string.IsNullOrWhiteSpace(settings.Certification))
            {
                var path = Path.IsPathRooted(settings.Certification)
                    ? settings.Certification
                    : Path.Combine(AppContext.BaseDirectory, settings.Certification);

                if (File.Exists(path))
                {
                    logger.LogInformation($"Loading Swish client certificate from file: {Path.GetFileName(path)}");
                    var all = new X509Certificate2Collection();
                    all.Import(path, settings.Passphrase, X509KeyStorageFlags.Exportable);

                    var leaf = all.OfType<X509Certificate2>().FirstOrDefault(c => c.HasPrivateKey);
                    if (leaf == null) throw new InvalidOperationException("Certificate file does not contain a private key.");

                    var intermediates = new X509Certificate2Collection(
                        all.OfType<X509Certificate2>().Where(c => !c.HasPrivateKey).ToArray());
                    return (leaf, intermediates);
                }

                logger.LogWarning($"Configured certificate file '{settings.Certification}' not found; falling back to certificate store by thumbprint.");
            }

            if (string.IsNullOrWhiteSpace(settings.Thumbprint))
            {
                logger.LogError("No certificate file found and no thumbprint configured.");
                throw new InvalidOperationException("A certificate file (Certification) or a Thumbprint must be configured.");
            }

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);

                logger.LogInformation($"Searching for certificate with thumbprint ending in: {settings.Thumbprint[^6..]}");

                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, settings.Thumbprint, false);
                if (certs.Count == 0) throw new InvalidOperationException($"No certificate found with given thumbprint");

                var selectedCert = certs.OfType<X509Certificate2>().FirstOrDefault();
                if (selectedCert == null || !selectedCert.HasPrivateKey) throw new InvalidOperationException($"Certificate does not have a private key.");

                logger.LogInformation($"Certificate found: {selectedCert.Subject}");
                return (selectedCert, new X509Certificate2Collection());
            }
        }

    }
}

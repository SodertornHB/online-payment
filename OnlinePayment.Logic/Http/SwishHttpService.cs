using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OnlinePayment.Logic.Model;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Settings;

namespace OnlinePayment.Logic.Http
{
    public interface ISwishHttpService : IHttpService<PaymentRequest>
    {
        Task<PaymentResponse> Put(string id, PaymentRequest model);
    }
    public class SwishHttpService : HttpService<PaymentRequest>, ISwishHttpService
    {
        private readonly SwishApiSettings swishApiSettings;
        public SwishHttpService(ISwishHttpClient client,
            ILogger<SwishHttpService> logger,
           IOptions<SwishApiSettings> options)
            : base(client, logger)
        {
            swishApiSettings = options.Value;
        }
        public virtual async Task<PaymentResponse> Put(string id, PaymentRequest model)
        {
            try
            {
                var content = JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var uri = CombineUrls($"{swishApiSettings.Endpoint}/api/v2/paymentrequests", id);
                var response = await client.Put(uri, content);
                response.CheckStatus();
                logger.LogDebug($"Put data from {uri}: {response.Content}");
                var paymentResponse = new PaymentResponse
                {
                    Session = model.Session,
                    Location = response.Headers.Location?.ToString() ?? string.Empty,
                    PaymentResponseReceivedDateTime = DateTime.Now,
                };
                return paymentResponse;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                throw;
            }
        }

    }
}

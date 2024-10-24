using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OnlinePayment.Logic.Model;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Settings;
using OnlinePayment.Logic.Services;

namespace OnlinePayment.Logic.Http
{
    public interface ISwishHttpService : IHttpService<PaymentRequest>
    {
        Task<PaymentResponse> Put(string id, PaymentRequest model);
    }
    public class SwishHttpService : HttpService<PaymentRequest>, ISwishHttpService
    {
        private readonly SwishApiSettings swishApiSettings;
        private readonly IAuditService auditService;

        public SwishHttpService(ISwishHttpClient client,
            ILogger<SwishHttpService> logger,
           IOptions<SwishApiSettings> options,
           IAuditService auditService)
            : base(client, logger)
        {
            swishApiSettings = options.Value;
            this.auditService = auditService;
        }
        public virtual async Task<PaymentResponse> Put(string instructionUUID, PaymentRequest model)
        {
            try
            {
                var content = JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var uri = CombineUrls($"{swishApiSettings.Endpoint}/api/v2/paymentrequests", instructionUUID);
                var response = await client.Put(uri, content);
                await auditService.Insert(new Audit("Payment request send", model.Session, typeof(PaymentRequest)));
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

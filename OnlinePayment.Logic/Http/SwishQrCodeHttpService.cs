using Microsoft.Extensions.Logging;
using OnlinePayment.Logic.Model;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OnlinePayment.Logic.Http
{
    public interface ISwishQrCodeHttpService
    {
        Task<string> Get(string token);

    }
    public class SwishQrCodeHttpService : HttpService<string>, ISwishQrCodeHttpService
    {
        private readonly SwishQrCodeApiSettings swishApiSettings;
        public SwishQrCodeHttpService(ISwishHttpClient client,
            ILogger<HttpService<string>> logger,
           IOptions<SwishQrCodeApiSettings> options)
            : base(client, logger)
        {
            swishApiSettings = options.Value;
        }

        public override Task<string> Get(string url, string id)
        {
            return base.Get(url, id);
        }

        public new async Task<string> Get(string token)
        {
            var model = new QrCodeRequestModel
            {
                Format = "svg",
                Size = 300,
                Token = token
            };

            var content = JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var response = await client.Post(new Uri(swishApiSettings.Endpoint), content);
            return response.Content;
        }
    }
}

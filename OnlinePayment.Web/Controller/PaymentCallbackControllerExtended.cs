using AutoMapper;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json;
using System;
using Web.Controllers;

namespace OnlinePayment.Web.Controllers
{
    public partial class PaymentCallbackController 
    {
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromServices] IPaymentCallbackServiceExtended callbackService,
         [FromServices] ILogger<HomeController> logger,
         [FromServices] IMapper mapper,
         [FromBody] dynamic requestModel)
        {
            try
            {
                string serializedModel = string.Empty;

                if (requestModel is JsonElement jsonElement)
                {
                    serializedModel = jsonElement.ToString();
                }
                else
                {
                    serializedModel = JsonConvert.SerializeObject(requestModel, Formatting.Indented);
                }
                logger.LogInformation($"Callback received model: {serializedModel}");

                var callbackModel = JsonConvert.DeserializeObject<CallbackRequestModel>(requestModel.ToString());
                await callbackService.Insert(mapper.Map<PaymentCallback>(callbackModel), callbackModel.Id);

                return Ok(serializedModel);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error in Callback: {e.Message}");
                return BadRequest(e.Message);
            }
        }
    }
}
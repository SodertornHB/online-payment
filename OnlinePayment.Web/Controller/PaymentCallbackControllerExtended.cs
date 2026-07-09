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
                // Never log or echo the callback payload: it contains the payer's
                // alias (phone number), amount and payment references (finding 9).
                var callbackModel = JsonConvert.DeserializeObject<CallbackRequestModel>(requestModel.ToString());
                logger.LogInformation($"Callback received for payment id {callbackModel.Id}, status {callbackModel.Status}");
                await callbackService.Insert(mapper.Map<PaymentCallback>(callbackModel), callbackModel.Id);

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error in Callback: {e.Message}");
                return BadRequest();
            }
        }
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public partial class HomeController
    {
        private const string CALLBACK_NULL_MSG = "Callback hit but model was null";

        [HttpGet("init")]
        public async Task<IActionResult> Init([FromServices] IKohaService kohaService, int borrowerNumber)
        {
            try
            {
                var patron = await kohaService.GetPatron(borrowerNumber);
                var account = await kohaService.GetAccount(borrowerNumber);
                return View(new InitPayViewModel { BorrowerNumber = borrowerNumber, PatronName = patron.GetFullname(), PatronPhoneNumber = patron.GetPhone(), PatronEmail = patron.email, Amount = account.GetBalance() });
            }
            catch (ArgumentException e)
            {
                return View(new InitPayViewModel { Feedback = $"Unable to initialize payment: {e.Message}" });
            }
        }
        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromServices] IPaymentServiceExtended paymentServiceExtended, int borrowerNumber)
        {
            var payment = await paymentServiceExtended.InitiatePayment(borrowerNumber);
            return View(new PayViewModel { Session = payment.Session, Status = payment.Status });
        }

        [HttpGet("list")]
        public async Task<IActionResult> List([FromServices] IPaymentServiceExtended paymentServiceExtended,
            [FromServices] IMapper mapper)
        {
            var payments = await paymentServiceExtended.GetAll();
            var viewModel = mapper.Map<IEnumerable<PaymentViewModel>>(payments);
            return View(viewModel);
        }


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

        [HttpGet("session/{session}")]
        [HttpGet("home/session/{session}")]
        public async Task<IActionResult> Session([FromServices] IPaymentServiceExtended paymentServiceExtended,
            [FromServices] IMapper mapper, [FromServices] IAuditService auditService, string session)
        {
            var payment = await paymentServiceExtended.GetBySessionId(session);
            var viewModel = mapper.Map<SessionViewModel>(payment);
            var audits = await GetAudistsBySession(auditService, session);
            viewModel.Audits = mapper.Map<IEnumerable<AuditViewModel>>(audits);
            return View(viewModel);
        }


        [HttpGet("paid")]
        public IActionResult Paid() => View();

        [HttpGet("declined")]
        public IActionResult Declined() => View();

        [HttpGet("cancelled")]
        public IActionResult Cancelled() => View();



        private static async Task<IEnumerable<Audit>> GetAudistsBySession(IAuditService auditService, string session)
        {
            var audits = await auditService.GetAll();
            return audits.Where(x => x.BelongsToSameSession(session));
        }
    }
}
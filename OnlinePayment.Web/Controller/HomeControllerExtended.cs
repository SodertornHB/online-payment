using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using Sh.Library.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public partial class HomeController
    {

        // test url: https://localhost:53271/pay?borrowerNumber=123
        [NoLibraryAuth]
        [HttpGet("pay")]
        public async Task<IActionResult> Pay([FromServices] IPaymentServiceExtended paymentServiceExtended, int borrowerNumber)
        {
            var payment = await paymentServiceExtended.InitiatePayment(borrowerNumber);
            return View(new PayViewModel { Session = payment.Session, Status = payment.Status });
        }

        [NoLibraryAuth]
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromServices] IPaymentServiceExtended paymentServiceExtended, int borrowerNumber)
        {
            var payment = await paymentServiceExtended.InitiatePayment(borrowerNumber);
            return View(new PayViewModel { Session = payment.Session, Status = payment.Status });
        }

#if DEBUG
        [NoLibraryAuth]
#endif
        [HttpGet("list")]
        public async Task<IActionResult> List([FromServices] IPaymentServiceExtended paymentServiceExtended,
            [FromServices] IMapper mapper)
        {
            var payments = await paymentServiceExtended.GetAll();
            var viewModel = mapper.Map<IEnumerable<PaymentViewModel>>(payments);
            return View(viewModel);
        }



#if DEBUG
        [NoLibraryAuth]
#endif
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromServices] IPaymentServiceExtended paymentServiceExtended,
            [FromServices] ILogger<HomeController> logger,
            [FromBody] PaymentCallbackViewModel viewModel)
        {
            try
            {
                if (viewModel == null) logger.LogInformation($"Callback hit but viewmodel was null");
                else logger.LogInformation($"Callback: {JsonConvert.SerializeObject(viewModel)}");
                return Ok();
            }
            catch (System.Exception  e)
            {
                logger.LogError(e, e.Message);
                return BadRequest(e.Message);
            }
        }

#if DEBUG
        [NoLibraryAuth]
#endif
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
        [NoLibraryAuth]
        public IActionResult Paid() => View();

        [HttpGet("declined")]
        [NoLibraryAuth]
        public IActionResult Declined() => View();

        [HttpGet("cancelled")]
        [NoLibraryAuth]
        public IActionResult Cancelled() => View();



        private static async Task<IEnumerable<Audit>> GetAudistsBySession(IAuditService auditService, string session)
        {
            var audits = await auditService.GetAll();
            return audits.Where(x => x.BelongsToSameSession(session));
        }
    }
}
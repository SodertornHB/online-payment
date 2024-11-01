using AutoMapper;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace OnlinePayment.Web.Controllers
{
    [Route("[controller]")]
    public class SessionController : Controller
    {

        [HttpGet("{session}")]
        public async Task<IActionResult> Session([FromServices] IPaymentServiceExtended paymentServiceExtended,
            [FromServices] IMapper mapper, [FromServices] IAuditService auditService, string session)
        {
            var payment = await paymentServiceExtended.GetBySessionId(session);
            var viewModel = mapper.Map<SessionViewModel>(payment);
            var audits = await GetAudistsBySession(auditService, session);
            viewModel.Audits = mapper.Map<IEnumerable<AuditViewModel>>(audits);
            return View(viewModel);
        }

        private static async Task<IEnumerable<Audit>> GetAudistsBySession(IAuditService auditService, string session)
        {
            var audits = await auditService.GetAll();
            return audits.Where(x => x.BelongsToSameSession(session));
        }
    }
}
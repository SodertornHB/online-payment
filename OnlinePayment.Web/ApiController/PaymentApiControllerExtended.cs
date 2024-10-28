using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Logic.Services;
using Sh.Library.Authentication;
using System.Threading.Tasks;

namespace OnlinePayment.Web.ApiController
{
    public partial class PaymentController
    {

        [NoLibraryAuth]
        [HttpGet("session/{sessionId}")]
        public virtual async Task<IActionResult> Get([FromServices] IPaymentServiceExtended paymentServiceExtended, 
           string sessionId)
        {

            var payment = await paymentServiceExtended.GetBySessionId(sessionId);
            if (payment == null) return NotFound();
            return Ok(payment);
        }
    }
}
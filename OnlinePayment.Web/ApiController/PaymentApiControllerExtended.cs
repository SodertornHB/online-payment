using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Logic.Services;
using System.Threading.Tasks;

namespace OnlinePayment.Web.ApiController
{
    public partial class PaymentController
    {

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
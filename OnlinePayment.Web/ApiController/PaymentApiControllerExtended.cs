using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Logic.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnlinePayment.Web.ApiController
{
    public partial class PaymentController
    {

        [HttpGet("session/{sessionId}")]
        public virtual async Task<IActionResult> Get([FromServices] IPaymentServiceExtended paymentServiceExtended, 
           string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return Ok("Session id was null or empty");

            if (!Regex.IsMatch(sessionId, @"^[a-fA-F0-9]{32}$")) return Ok("Session id was in incorrect format"); 

            var payment = await paymentServiceExtended.GetBySessionId(sessionId);
            if (payment == null) return NotFound();
            return Ok(payment);
        }
    }
}
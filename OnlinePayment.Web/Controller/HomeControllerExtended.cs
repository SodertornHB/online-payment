using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Services;
using OnlinePayment.Logic.Settings;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public partial class HomeController
    {
        /// <remarks>
        /// To fetch this javascript into Koha, add the following in `OPACUserJS` system preference:
        /// var paymentScript = document.createElement("script");
        /// paymentScript.type = "text/javascript";
        /// paymentScript.src = "https://YOUR_HOST/js?borrowernumber=" + $('.loggedinusername').attr('data-borrowernumber') + "&lang=" + $('html').attr('lang');
        /// $("head").append(paymentScript);
        /// </remarks>
        [HttpGet("js")]
        public async Task<IActionResult> js([FromServices] IOptions<ApplicationSettings> applicationSettinsOptions,
            [FromServices] IKohaService kohaService,
            int borrowerNumber, string lang)
        {
            if (borrowerNumber == default) return Ok();

            var account = await kohaService.GetAccount(borrowerNumber);
            if (account.balance < 1) return Ok();

            var msg = lang == "sv-SE" ? "Betala avgifter" : "Pay fees";
            var altMsg = lang == "sv-SE" ? "Betala med Swish" : "Pay with Swish";

            var applicationHost = applicationSettinsOptions.Value.Host;
            var applicationName = applicationSettinsOptions.Value.Name;
            var fullHost = $"{applicationHost}{applicationName}";

            var initUrl = $"{fullHost}/init?borrowerNumber={borrowerNumber}";
            var imgUrl = $"{fullHost}/img/swish_small.png";

            string js = @$"
                        $(document).ready(function() {{
                            $('#useraccount').after(`
                         <div style=""margin: 20px 0; width: 100px; text-align: center;"">
                                <a href='{initUrl}'>                          
                                <span style=""font-size: 0.8em;"">{msg}</span>                            
                                <img src=""{imgUrl}"" alt=""{altMsg}"" style=""margin:10px"">
                            </a>
                        </div>
                            `);
                        }});";

            return Ok(js);
        }

        [HttpGet("paid")]
        public IActionResult Paid() => View();

        [HttpGet("declined")]
        public IActionResult Declined() => View();

        [HttpGet("cancelled")]
        public IActionResult Cancelled() => View();
    }
}
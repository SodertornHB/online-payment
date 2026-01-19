using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Services;
using OnlinePayment.Logic.Settings;
using Sh.Library.Authentication;
using System.Collections.Generic;
using System.Linq;
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
        [NoLibraryAuth]
        [HttpGet("js")]
        public async Task<IActionResult> js([FromServices] IOptions<ApplicationSettings> applicationSettinsOptions,
        [FromServices] IKohaService kohaService,
        [FromServices] ILogger<HomeController> logger,
        int borrowerNumber, string lang)
        {
            try
            {
                var applicationSettings = applicationSettinsOptions.Value;
                if (borrowerNumber == default) return Ok();

                var account = await kohaService.GetAccount(borrowerNumber);
                if (account.GetBalanceForGivenStatuses(applicationSettings.StatusesGeneratingPaymentBalance) < 1) return Ok();

                var msg = lang == "sv-SE" ? "Betala avgifter" : "Pay fees";
                var altMsg = lang == "sv-SE" ? "Betala med Swish" : "Pay with Swish";

                var applicationHost = applicationSettings.Host;
                var applicationName = applicationSettings.Name;
                var fullHost = $"{applicationHost}{applicationName}";

                var initUrl = $"{fullHost}/init?borrowerNumber={borrowerNumber}";
                var imgUrl = $"{fullHost}/img/swish_small.png";

                string js = @$"
                $(document).ready(function() {{
                    $('#finestable').before(`
                <div style='margin: 20px 0; width: 100px; text-align: center;float: right;'>
                        <a href='{initUrl}'>                          
                        <span style='font-size: 0.8em;'>{msg}</span>                            
                        <img src='{imgUrl}' alt='{altMsg}' style='margin:10px'>
                    </a>
                </div>
                    `);
                }});";

                return Ok(js);
            }
            catch (System.ArgumentException e)
            {
                logger.LogWarning(e, "Could not generate payment JS for borrowerNumber {borrowerNumber}", borrowerNumber);
                return Ok();
            }
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
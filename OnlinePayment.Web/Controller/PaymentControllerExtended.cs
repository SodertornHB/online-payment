using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using OnlinePayment.Web.Security;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;
using System;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Settings;
using Microsoft.Extensions.Logging;

namespace OnlinePayment.Web.Controllers
{
    public partial class PaymentController
    {
        [HttpGet("init")]
        [EnableRateLimiting(PaymentRateLimit.Policy)]
        public async Task<IActionResult> Init([FromServices] IKohaService kohaService,
            [FromServices] IOptions<ApplicationSettings> applicationSettinsOptions,
            [FromServices] IBorrowerTokenService borrowerTokenService, string token, bool @internal)
        {
            if (!borrowerTokenService.TryResolve(token, out var borrowerNumber))
                return View(new InitPayViewModel { Feedback = "Unable to initialize payment: the link is invalid or has expired." });

            try
            {
                var applicationsSettings = applicationSettinsOptions.Value;
                var patron = await kohaService.GetPatron(borrowerNumber);
                var account = await kohaService.GetAccount(borrowerNumber);
                var balance = account.GetBalanceForGivenStatuses(applicationsSettings.StatusesGeneratingPaymentBalance);
                // Do not log borrower id, amount or balance (finding 9).
                logger.LogInformation("Init payment view rendered for borrower resolved from token");
                return base.View(new InitPayViewModel { Token = token, PatronName = patron.GetFullname(), Amount = balance, ShowPaymentButton = !@internal });
            }
            catch (ArgumentException e)
            {
                return View(new InitPayViewModel { Feedback = $"Unable to initialize payment: {e.Message}" });
            }
        }

        [HttpPost("pay")]
        [EnableRateLimiting(PaymentRateLimit.Policy)]
        public async Task<IActionResult> Pay([FromServices] IPaymentServiceExtended paymentServiceExtended,
            [FromServices] IBorrowerTokenService borrowerTokenService, InitPayViewModel viewModel)
        {
            if (!borrowerTokenService.TryResolve(viewModel.Token, out var borrowerNumber))
                return RedirectToAction("Cancelled", "Home");

            try
            {
                var payment = await paymentServiceExtended.InitiatePayment(borrowerNumber);

                return View(new PayViewModel { Session = payment.Session, Status = payment.Status });
            }
            catch (ArgumentException e)
            {
                logger.LogWarning($"ArgumentException: {e.Message}");
                return RedirectToAction("Cancelled", "Home");
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> List([FromServices] IPaymentServiceExtended paymentServiceExtended,
            [FromServices] IMapper mapper)
        {
            var payments = await paymentServiceExtended.GetAll();
            var viewModel = mapper.Map<IEnumerable<PaymentViewModel>>(payments);
            return View(viewModel);
        }
    }
}
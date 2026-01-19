using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using Sh.Library.Authentication;
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
        [NoLibraryAuth]
        [HttpGet("init")]
        public async Task<IActionResult> Init([FromServices] IKohaService kohaService,
            [FromServices] IOptions<ApplicationSettings> applicationSettinsOptions, int borrowerNumber, bool @internal)
        {
            try
            {
                var applicationsSettings = applicationSettinsOptions.Value;
                var patron = await kohaService.GetPatron(borrowerNumber);
                var account = await kohaService.GetAccount(borrowerNumber);
                var balance = account.GetBalanceForGivenStatuses(applicationsSettings.StatusesGeneratingPaymentBalance);
                logger.LogInformation($"Init payment for borrower {patron.patron_id}, amount to pay = {balance}, total balance = {account.balance}");
                return base.View(new InitPayViewModel { BorrowerNumber = borrowerNumber, PatronName = patron.GetFullname(), PatronPhoneNumber = patron.GetPhone(), PatronEmail = patron.email, Amount = balance, ShowPaymentButton = !@internal });
            }
            catch (ArgumentException e)
            {
                return View(new InitPayViewModel { Feedback = $"Unable to initialize payment: {e.Message}" });
            }
        }

        [NoLibraryAuth]
        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromServices] IPaymentServiceExtended paymentServiceExtended, InitPayViewModel viewModel)
        {
            try
            {
                var payment = await paymentServiceExtended.InitiatePayment(viewModel.BorrowerNumber);

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
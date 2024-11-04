using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using System.Threading.Tasks;
using System;
using AutoMapper;
using System.Collections.Generic;

namespace OnlinePayment.Web.Controllers
{
    public partial class PaymentController 
    {
        [HttpGet("init")]
        public async Task<IActionResult> Init([FromServices] IKohaService kohaService, int borrowerNumber)
        {
            try
            {
                var patron = await kohaService.GetPatron(borrowerNumber);
                var account = await kohaService.GetAccount(borrowerNumber);
                var balance = account.GetBalance();
                return View(new InitPayViewModel { BorrowerNumber = borrowerNumber, PatronName = patron.GetFullname(), PatronPhoneNumber = patron.GetPhone(), PatronEmail = patron.email, Amount = balance });
            }
            catch (ArgumentException e)
            {
                return View(new InitPayViewModel { Feedback = $"Unable to initialize payment: {e.Message}" });
            }
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay([FromServices] IPaymentServiceExtended paymentServiceExtended, InitPayViewModel viewModel)
        {
            var payment = await paymentServiceExtended.InitiatePayment(viewModel.BorrowerNumber);

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
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Logic.Services;
using OnlinePayment.Web.ViewModel;
using Sh.Library.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public partial class HomeController
    {

        // test url: https://localhost:53271/pay?borrowerNumber=123&patronName=John%20Doe&patronEmail=johndoe%40example.com&patronPhoneNumber=1234567890&amount=50,75
        [NoLibraryAuth]
        [HttpGet("pay")]
        public async Task<IActionResult> Pay([FromServices] IPaymentServiceExtended paymentServiceExtended,
            int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount)
        {
            var viewModel = new PayViewModel
            {
                BorrowerNumber = borrowerNumber,
                PatronName = patronName ?? "",
                PatronEmail = patronEmail ?? "",
                PatronPhoneNumber = patronPhoneNumber ?? "",
                Amount = amount
            };

            var payment = await paymentServiceExtended.InitiatePayment(borrowerNumber, patronName, patronEmail, patronPhoneNumber, amount);
            viewModel.QrCode = payment.QrCode;
            return View(viewModel);
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
        [HttpGet("session")]
        public async Task<IActionResult> Session([FromServices] IPaymentServiceExtended paymentServiceExtended,
            [FromServices] IMapper mapper, [FromQuery] string session)
        {
            var payments = await paymentServiceExtended.GetAll(); //.GetSession(session);
            var viewModel = mapper.Map<IEnumerable<PaymentViewModel>>(payments);
            return View(viewModel);
        }
    }
}
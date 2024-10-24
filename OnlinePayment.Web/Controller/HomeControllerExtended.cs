using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Web.ViewModel;
using Sh.Library.Authentication;
using System.IO;
using System;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public partial class HomeController
    {

        // test url: https://localhost:53271/pay?borrowerNumber=123&patronName=John%20Doe&patronEmail=johndoe%40example.com&patronPhoneNumber=1234567890&amount=50,75
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

            var payment = await paymentServiceExtended.Initiate(borrowerNumber, patronName, patronEmail, patronPhoneNumber, amount);
            viewModel.QrCode = payment.QrCode;
            return View(viewModel);
        }


    }
}
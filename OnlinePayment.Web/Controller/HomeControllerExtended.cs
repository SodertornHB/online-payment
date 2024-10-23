using Microsoft.AspNetCore.Mvc;
using OnlinePayment.Web.ViewModel;
using Sh.Library.Authentication;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public partial class HomeController
    {

        // test url: https://localhost:53271/home/pay?borrowerNumber=123&patronName=John%20Doe&patronEmail=johndoe%40example.com&patronPhoneNumber=1234567890&amount=50.75
        [HttpGet("pay")]
        public IActionResult Pay(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, decimal amount)
        {
            var viewModel = new PayViewModel
            {
                BorrowerNumber = borrowerNumber,
                PatronName = patronName ?? "",
                PatronEmail = patronEmail ?? "",
                PatronPhoneNumber = patronPhoneNumber ?? "",
                Amount = amount
            };

            await paymentServiceExtended.Initiate(borrowerNumber, patronName, patronEmail, patronPhoneNumber, amount);

            return View(viewModel);
        }


    }
}
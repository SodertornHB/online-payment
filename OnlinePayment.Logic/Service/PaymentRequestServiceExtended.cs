using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Settings;
using System;
using System.Threading.Tasks;

namespace OnlinePayment.Logic.Services
{
    public interface IPaymentRequestServiceExtended : IPaymentRequestService
    {
        PaymentRequest CreatePaymentRequest(int borrowerNumber, string patronPhoneNumber, int amount, string session);
    }

    public partial class PaymentRequestServiceExtended : PaymentRequestService, IPaymentRequestServiceExtended
    {
        private readonly SwishApiSettings swishApiSettings;

        public PaymentRequestServiceExtended(ILogger<PaymentRequestService> logger,
           IPaymentRequestDataAccess dataAccess,
           IOptions<SwishApiSettings> options)
           : base(logger, dataAccess)
        {
            swishApiSettings = options.Value;
        }
        public override Task<PaymentRequest> Insert(PaymentRequest model)
        {
            if (!model.IsValid())
            {
                throw new FormatException("Amount must be a valid decimal number.");
            }

            model.ConvertAmountToInt();

            return base.Insert(model);
        }


        public PaymentRequest CreatePaymentRequest(int borrowerNumber, string patronPhoneNumber, int amount, string session)
        {
            var paymentRequest = new PaymentRequest
            {
                Session = session,
                PayeePaymentReference = borrowerNumber.ToString(), //internal ref 
                CallbackUrl = swishApiSettings.CallbackUrl,
                PayerAlias = patronPhoneNumber,
                PayeeAlias = swishApiSettings.PayeeAlias,
                Amount = $"{amount}.00",
                Currency = swishApiSettings.Currency,
                Message = "Avgift",
                PaymentRequestDateTime = DateTime.Now,
            };
            return paymentRequest;
        }

    }
}

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
        Task<PaymentRequest> CreatePaymentRequest(int borrowerNumber, string patronPhoneNumber, int amount, string session);
    }

    public partial class PaymentRequestServiceExtended : PaymentRequestService, IPaymentRequestServiceExtended
    {
        private readonly SwishApiSettings swishApiSettings;
        private readonly IAuditService auditService;

        public PaymentRequestServiceExtended(ILogger<PaymentRequestService> logger,
           IPaymentRequestDataAccess dataAccess,
           IOptions<SwishApiSettings> options,
           IAuditService auditService)
           : base(logger, dataAccess)
        {
            swishApiSettings = options.Value;
            this.auditService = auditService;
        }
        public override async Task<PaymentRequest> Insert(PaymentRequest model)
        {
            if (!model.IsValid())
            {
                throw new FormatException("Amount must be a valid decimal number.");
            }

            model.ConvertAmountToInt();

            var paymentRequest = await base.Insert(model);

            await auditService.Insert(new Audit("Payment request saved", model.Session, typeof(PaymentRequest)));
                        
            return paymentRequest;
        }


        public async Task<PaymentRequest> CreatePaymentRequest(int borrowerNumber, string patronPhoneNumber, int amount, string session)
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
            await Insert(paymentRequest);
            return paymentRequest;
        }

    }
}

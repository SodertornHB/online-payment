using OnlinePayment.Logic.DataAccess;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OnlinePayment.Logic.Model;
using Logic.Service;
using System;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Settings;
using OnlinePayment.Logic.Http;
using System.Linq;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentServiceExtended : IPaymentService
    {
        Task<Payment> Initiate(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount);
    }

    public partial class PaymentServiceExtended : PaymentService, IPaymentServiceExtended
    {
        private readonly SwishApiSettings swishApiSettings;
        private readonly IPaymentRequestService paymentRequestService;
        private readonly IPaymentResponseService paymentResponseService;
        private readonly ISwishHttpService swishHttpService;

        public PaymentServiceExtended(ILogger<PaymentService> logger,
           IPaymentDataAccess dataAccess,
           IPaymentRequestService paymentRequestService,
           IPaymentResponseService paymentResponseService,
           ISwishHttpService swishHttpService,
           IOptions<SwishApiSettings> options)
           : base(logger, dataAccess)
        {
            swishApiSettings = options.Value;
            this.paymentRequestService = paymentRequestService;
            this.paymentResponseService = paymentResponseService;
            this.swishHttpService = swishHttpService;
        }

        public async Task<Payment> Initiate(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount)
        {
            try
            {
                var session = GuidGenerator.GenerateGuidWithoutDashesUppercase();
                var guid = GuidGenerator.GenerateGuidWithoutDashesUppercase();
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
                await paymentRequestService.Insert(paymentRequest);

                var paymentResponse = await swishHttpService.Put(guid, paymentRequest);
                var paymentFromSwish = await swishHttpService.Get(paymentResponse.Location, string.Empty);
                paymentResponse.PaymentReference = paymentFromSwish.PaymentReference;
                paymentResponse.Status = paymentFromSwish.Status;
                await paymentResponseService.Insert(paymentResponse);

                var payment = new Payment
                {
                    ExternalId = paymentResponse.Location.Split('/').Last(),
                    Session = session,
                    BorrowerNumber = borrowerNumber,
                    PatronName = patronName,
                    PatronEmail = patronEmail,
                    PatronPhoneNumber = patronPhoneNumber,
                    Amount = amount,
                    InitiationDateTime = paymentRequest.PaymentRequestDateTime,
                    Status = paymentResponse.Status,
                    Description = string.Empty
                };

                await Insert(payment);

                return payment;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error message: " + e.Message);

                throw;
            }
        }
    }
}

using OnlinePayment.Logic.DataAccess;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OnlinePayment.Logic.Model;
using Logic.Service;
using System;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Settings;
using OnlinePayment.Logic.Http;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentServiceExtended : IPaymentService
    {
        Task<PaymentResponse> Initiate(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int Amount);
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

        public async Task<PaymentResponse> Initiate(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount)
        {
            try
            {
                var session = GuidGenerator.GenerateGuidWithoutDashesUppercase();
                var guid = GuidGenerator.GenerateGuidWithoutDashesUppercase();
                var paymentRequest = new PaymentRequest
                { 
                    Session = session,
                    PayeePaymentReference = "TEST123", //internal ref 
                    CallbackUrl = swishApiSettings.CallbackUrl,
                    PayerAlias = patronPhoneNumber,
                    PayeeAlias = swishApiSettings.PayeeAlias,
                    Amount = $"{amount}.00" ,
                    Currency = swishApiSettings.Currency,
                    Message = swishApiSettings.Message,
                    PaymentRequestDateTime = DateTime.Now,
                };
                await paymentRequestService.Insert(paymentRequest);

                var paymentResponse = await swishHttpService.Put(guid, paymentRequest);
                await paymentResponseService.Insert(paymentResponse);
                return paymentResponse;
            }
            catch (Exception e)
            {
                logger.LogError(e,"Error message: " + e.Message);

                throw;
            }
        }
    }
}

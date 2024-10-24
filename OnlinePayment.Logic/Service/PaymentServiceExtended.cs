using OnlinePayment.Logic.DataAccess;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OnlinePayment.Logic.Model;
using Logic.Service;
using System;
using OnlinePayment.Logic.Http;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Settings;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentServiceExtended : IPaymentService
    {
        Task<Payment> InitiatePayment(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount);
    }

    public partial class PaymentServiceExtended : PaymentService, IPaymentServiceExtended
    {
        private readonly IPaymentRequestServiceExtended paymentRequestService;
        private readonly IPaymentResponseService paymentResponseService;
        private readonly ISwishHttpService swishHttpService;
        private readonly ISwishQrCodeHttpService swishQrCodeHttpService;
        private readonly SwishApiSettings swishApiSettings;
        private new readonly IPaymentDataAccessExtended dataAccess;

        public PaymentServiceExtended(ILogger<PaymentService> logger,
           IPaymentDataAccessExtended dataAccess,
           IPaymentRequestServiceExtended paymentRequestService,
           IPaymentResponseService paymentResponseService,
           ISwishHttpService swishHttpService,
           ISwishQrCodeHttpService swishQrCodeHttpService,
           IOptions<SwishApiSettings> options)
           : base(logger, dataAccess)
        {
            this.paymentRequestService = paymentRequestService;
            this.paymentResponseService = paymentResponseService;
            this.swishHttpService = swishHttpService;
            this.swishQrCodeHttpService = swishQrCodeHttpService;
            this.swishApiSettings = options.Value;
            this.dataAccess = dataAccess;
        }

        public async Task<Payment> InitiatePayment(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount)
        {
            try
            {
                var session = GuidGenerator.GenerateGuidWithoutDashesUppercase();
                var instructionUUID = GuidGenerator.GenerateGuidWithoutDashesUppercase();
                var paymentRequest = paymentRequestService.CreatePaymentRequest(borrowerNumber, patronPhoneNumber, amount, session);
                await paymentRequestService.Insert(paymentRequest);

                var paymentResponse = await swishHttpService.Put(instructionUUID, paymentRequest);
                var paymentFromSwish = await swishHttpService.Get(paymentResponse.Location, string.Empty);
                paymentResponse.PaymentReference = paymentFromSwish.PaymentReference;
                paymentResponse.Status = paymentFromSwish.Status;
                await paymentResponseService.Insert(paymentResponse);
                var payment = await CreatePayment(borrowerNumber, patronName, patronEmail, patronPhoneNumber, amount, session, paymentRequest, paymentResponse);

                await Insert(payment);

                return payment;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error message: " + e.Message);

                throw;
            }
        }

        public override async Task<IEnumerable<Payment>> GetAll()
        {
            await UpdateStatusOnAll();
            return await base.GetAll();
        }

        #region private

        private async Task UpdateStatusOnAll()
        {
            var unpaidPayments = await dataAccess.GetUnpaid();
            foreach (var unpaidPayment in unpaidPayments)
            {
                var paymentFromSwish = await swishHttpService.Get($"{swishApiSettings.Endpoint}/api/v1/paymentrequests/", unpaidPayment.ExternalId);
                if (unpaidPayment.Status != paymentFromSwish.Status) await UpdateStatus(unpaidPayment, paymentFromSwish.Status);
            }

        }

        private async Task UpdateStatus(Payment unpaidPayment, string status)
        {
            unpaidPayment.Status = status;
            await Update(unpaidPayment);
        }

        private async Task<Payment> CreatePayment(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount, string session, PaymentRequest paymentRequest, PaymentResponse paymentResponse)
        {
            var externalId = paymentResponse.Location.Split('/').Last();
            var qr = await swishQrCodeHttpService.Get(externalId);
            return new Payment
            {
                ExternalId = externalId,
                Session = session,
                BorrowerNumber = borrowerNumber,
                PatronName = patronName,
                PatronEmail = patronEmail,
                PatronPhoneNumber = patronPhoneNumber,
                Amount = amount,
                InitiationDateTime = paymentRequest.PaymentRequestDateTime,
                Status = paymentResponse.Status,
                Description = string.Empty,
                QrCode = qr
            };
        }

        #endregion
    }
}

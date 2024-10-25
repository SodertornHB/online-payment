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
        Task<Payment> GetBySessionId(string session);
        Task<Payment> InitiatePayment(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount);
    }

    public partial class PaymentServiceExtended : PaymentService, IPaymentServiceExtended
    {
        private readonly IPaymentRequestServiceExtended paymentRequestService;
        private readonly IPaymentResponseService paymentResponseService;
        private readonly ISwishHttpService swishHttpService;
        private readonly IAuditService auditService;
        private readonly SwishApiSettings swishApiSettings;
        private new readonly IPaymentDataAccessExtended dataAccess;

        public PaymentServiceExtended(ILogger<PaymentService> logger,
           IPaymentDataAccessExtended dataAccess,
           IPaymentRequestServiceExtended paymentRequestService,
           IPaymentResponseService paymentResponseService,
           ISwishHttpService swishHttpService,
           IOptions<SwishApiSettings> options, 
           IAuditService auditService)
           : base(logger, dataAccess)
        {
            this.paymentRequestService = paymentRequestService;
            this.paymentResponseService = paymentResponseService;
            this.swishHttpService = swishHttpService;
            this.auditService = auditService;
            this.swishApiSettings = options.Value;
            this.dataAccess = dataAccess;
        }

        public async Task<Payment> InitiatePayment(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount)
        {
            try
            {
                var session = GuidGenerator.GenerateGuidWithoutDashesUppercase();
                var instructionUUID = GuidGenerator.GenerateGuidWithoutDashesUppercase();

                await auditService.Insert(new Audit("Initiating", session, typeof(Payment)));

                var paymentRequest = await paymentRequestService.CreatePaymentRequest(borrowerNumber, patronPhoneNumber, amount, session);

                var paymentResponse = await swishHttpService.Put(instructionUUID, paymentRequest);

                await UpdatePaymentResponseAndInsert(paymentResponse, session);

                return await CreatePayment(borrowerNumber, patronName, patronEmail, patronPhoneNumber, amount, session, paymentRequest.PaymentRequestDateTime.Value, paymentResponse.Location, paymentResponse.Status);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error message: " + e.Message);
                throw;
            }
        }

        private async Task<Payment> CreatePayment(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount, string session, DateTime initDateTime, string location, string status)
        {
            var payment = await CreatePaymentEntityWithQrCode(borrowerNumber, patronName, patronEmail, patronPhoneNumber, amount, session, initDateTime, location, status);
            await auditService.Insert(new Audit("Payment saved", session, typeof(Payment)));
            await Insert(payment);
            return payment;
        }

        private async Task UpdatePaymentResponseAndInsert(PaymentResponse paymentResponse, string session)
        {
            var paymentFromSwish = await swishHttpService.Get(paymentResponse.Location, string.Empty);
            paymentResponse.PaymentReference = paymentFromSwish.PaymentReference;
            paymentResponse.Status = paymentFromSwish.Status;
            await auditService.Insert(new Audit($"Payment response with status {paymentResponse.Status}", session, typeof(PaymentResponse)));
            await paymentResponseService.Insert(paymentResponse);
        }

        public override async Task<IEnumerable<Payment>> GetAll()
        {
            await UpdateStatusOnAll();
            return await base.GetAll();
        }

        public async Task<Payment> GetBySessionId(string session)
        {
            var payment = await dataAccess.GetBySessionId(session);
            await UpdateStatusIfChanged(payment);
            return payment;
        }

        #region private

        private async Task UpdateStatusIfChanged(Payment payment)
        {
            var paymentFromSwish = await swishHttpService.Get($"{swishApiSettings.Endpoint}/api/v1/paymentrequests/", payment.ExternalId);
            if (payment.Status != paymentFromSwish.Status) await UpdateStatus(payment, paymentFromSwish.Status);
        }

        private async Task UpdateStatusOnAll()
        {
            var unpaidPayments = await dataAccess.GetUnpaid();
            foreach (var unpaidPayment in unpaidPayments)
            {
                await UpdateStatusIfChanged(unpaidPayment);
            }
        }

        private async Task UpdateStatus(Payment unpaidPayment, string status)
        {
            var oldStatus = unpaidPayment.Status;
            unpaidPayment.Status = status;
            await Update(unpaidPayment);
            await auditService.Insert(new Audit($"Status has been changed from {oldStatus} to {status}", unpaidPayment.Session, typeof(Payment)));
        }

        private async Task<Payment> CreatePaymentEntityWithQrCode(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount, string session, 
            DateTime initDateTime, string location, string status)
        {
            var externalId = location.Split('/').Last();
            return new Payment
            {
                ExternalId = externalId,
                Session = session,
                BorrowerNumber = borrowerNumber,
                PatronName = patronName,
                PatronEmail = patronEmail,
                PatronPhoneNumber = patronPhoneNumber,
                Amount = amount,
                InitiationDateTime = initDateTime,
                Status = status,
                Description = string.Empty
            };
        }

        #endregion
    }
}

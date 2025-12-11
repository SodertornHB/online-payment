using OnlinePayment.Logic.DataAccess;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using OnlinePayment.Logic.Model;
using Logic.Service;
using System;
using OnlinePayment.Logic.Http;
using System.Linq;
using Microsoft.Extensions.Options;
using OnlinePayment.Logic.Settings;
using System.Text.RegularExpressions;

namespace OnlinePayment.Logic.Services
{
    public partial interface IPaymentServiceExtended : IPaymentService
    {
        Task<Payment> GetByExternalId(string externalId);
        Task<Payment> GetBySessionId(string session);
        Task<Payment> InitiatePayment(int borrowerNumber);
    }

    public partial class PaymentServiceExtended : PaymentService, IPaymentServiceExtended
    {
        private readonly IPaymentRequestServiceExtended paymentRequestService;
        private readonly IPaymentResponseService paymentResponseService;
        private readonly ISwishHttpService swishHttpService;
        private readonly IAuditServiceExtended auditService;
        private readonly IKohaService kohaService;
        private readonly ApplicationSettings applicationSettings;
        private readonly SwishApiSettings swishApiSettings;
        private new readonly IPaymentDataAccessExtended dataAccess;

        public PaymentServiceExtended(ILogger<PaymentService> logger,
           IPaymentDataAccessExtended dataAccess,
           IPaymentRequestServiceExtended paymentRequestService,
           IPaymentResponseService paymentResponseService,
           ISwishHttpService swishHttpService,
           IOptions<SwishApiSettings> options,
           IAuditServiceExtended auditService,
           IKohaService kohaService,
           IOptions<ApplicationSettings> applicationSettingsOptions)
           : base(logger, dataAccess)
        {
            this.paymentRequestService = paymentRequestService;
            this.paymentResponseService = paymentResponseService;
            this.swishHttpService = swishHttpService;
            this.auditService = auditService;
            this.kohaService = kohaService;
            this.applicationSettings = applicationSettingsOptions.Value;
            this.swishApiSettings = options.Value;
            this.dataAccess = dataAccess;
        }   

        public async Task<Payment> InitiatePayment(int borrowerNumber)
        {
            logger.LogInformation($"Initiate payment process for borrower with number {borrowerNumber}");
            var session = GuidGenerator.GenerateGuidWithoutDashesUppercase();
            logger.LogInformation($"Generated session guid = {session}");
            await auditService.AddAudit("Initiating", session, typeof(Payment));
            var patron = await kohaService.GetPatron(borrowerNumber, session);
            var account = await kohaService.GetAccount(borrowerNumber, session);
            Payment payment = await InitiatePayment(session, borrowerNumber, $"{patron.firstname} {patron.surname}", patron.email, patron.GetPhone(), account.GetBalanceForGivenStatuses(applicationSettings.StatusesGeneratingPaymentBalance));
            return payment;
        }

        public async Task<Payment> GetByExternalId(string externalId)
        {
            var payment = await dataAccess.GetByExternalId(externalId);
            await UpdateStatusIfChanged(payment);
            return payment;
        }

        public async Task<Payment> GetBySessionId(string session)
        {
            if (!Regex.IsMatch(session, @"^[a-fA-F0-9]{32}$"))throw new ArgumentException("Invalid session ID format. It should be a GUID without dashes.");
            
            var payment = await dataAccess.GetBySessionId(session);
            await UpdateStatusIfChanged(payment);
            return payment;
        }

        #region private

        private async Task<Payment> InitiatePayment(string session, int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount)
        {
            try
            {
                var instructionUUID = GuidGenerator.GenerateGuidWithoutDashesUppercase();

                PaymentRequest paymentRequest = await paymentRequestService.CreatePaymentRequest(borrowerNumber, patronPhoneNumber, amount, session);

                await Log($"Preparing to create payment in Swish. Borrowernumber = {borrowerNumber}, Amout = {amount}", paymentRequest);

                var paymentResponse = await swishHttpService.Put(instructionUUID, paymentRequest);

                await Log($"Payment created in Swish. Borrowernumber = {borrowerNumber}, Amout = {amount}", paymentRequest);

                await UpdatePaymentResponseAndInsert(paymentResponse, session);

                var payment = await CreatePayment(borrowerNumber, patronName, patronEmail, patronPhoneNumber, amount, session, paymentRequest.PaymentRequestDateTime.Value, paymentResponse.Location, paymentResponse.Status);

                await Log($"Payment created. Borrowernumber = {borrowerNumber}, Amout = {amount}, paymentRequest.PaymentRequestDateTime.Value = {paymentRequest.PaymentRequestDateTime.Value}, paymentResponse.Location = {paymentResponse.Location}, paymentResponse.Status = { paymentResponse.Status}", paymentRequest);

                return payment;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error message: " + e.Message);
                if (e.InnerException != null) logger.LogError(e.InnerException, "InnerException: " + e.InnerException.Message);
                throw;
            }
        }


        private async Task Log(string prepareMessage, PaymentRequest paymentRequest)
        {
            logger.LogInformation(prepareMessage);
            await auditService.AddAudit(prepareMessage, paymentRequest.Session, typeof(PaymentRequest));
        }

        private async Task<Payment> CreatePayment(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount, string session, DateTime initDateTime, string location, string status)
        {
            var payment = CreatePaymentEntityWithQrCode(borrowerNumber, patronName, patronEmail, patronPhoneNumber, amount, session, initDateTime, location, status);
            await auditService.AddAudit("Payment saved", session, typeof(Payment));
            await Insert(payment);
            return payment;
        }

        private async Task UpdatePaymentResponseAndInsert(PaymentResponse paymentResponse, string session)
        {
            var paymentFromSwish = await swishHttpService.Get(paymentResponse.Location, string.Empty);
            paymentResponse.PaymentReference = paymentFromSwish.PaymentReference;
            paymentResponse.Status = paymentFromSwish.Status;
            await auditService.AddAudit($"Payment response with status {paymentResponse.Status}", session, typeof(PaymentResponse));
            await paymentResponseService.Insert(paymentResponse);
        }

        private async Task UpdateStatusIfChanged(Payment payment)
        {
            var paymentFromSwish = await swishHttpService.Get($"{swishApiSettings.Endpoint}/api/v1/paymentrequests/", payment.ExternalId);
            if (payment.Status != paymentFromSwish.Status) await UpdateStatus(payment, paymentFromSwish.Status);
        }

        private async Task UpdateStatus(Payment unpaidPayment, string status)
        {
            var oldStatus = unpaidPayment.Status;
            unpaidPayment.Status = status;
            await Update(unpaidPayment);
            await auditService.Insert(new Audit($"Status has been changed from {oldStatus} to {status}", unpaidPayment.Session, typeof(Payment)));
        }

        private Payment CreatePaymentEntityWithQrCode(int borrowerNumber, string patronName, string patronEmail, string patronPhoneNumber, int amount, string session, 
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

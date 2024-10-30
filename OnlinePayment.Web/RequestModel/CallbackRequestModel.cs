using System;
using System.ComponentModel.DataAnnotations;

namespace OnlinePayment.Web.ViewModel
{
    public class CallbackRequestModel
    {
        public string Id { get; set; } = "";
        public string PaymentReference {get;set;}  = ""; 
        public string PayeePaymentReference {get;set;}  = ""; 
        public string CallbackUrl {get;set;}  = ""; 
        public string CallbackIdentifier {get;set;}  = ""; 
        public string Status {get;set;}  = ""; 
        public decimal Amount {get;set;} 
        public string Currency {get;set;}  = ""; 
        public string PayerAlias {get;set;}  = ""; 
        public string PayeeAlias {get;set;}  = ""; 
        public string Message {get;set;}  = ""; 
        public string ErrorMessage {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public DateTime? DateCreated {get;set;} 
        [DataType(DataType.Text)]
        public DateTime? DatePaid {get;set;} 
        public string ErrorCode {get;set;}  = ""; 
    }
} 
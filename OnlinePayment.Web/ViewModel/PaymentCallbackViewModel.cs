
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace OnlinePayment.Web.ViewModel
{
    public partial class PaymentCallbackViewModel : ViewModelBase
    {
        public virtual string Session {get;set;}  = ""; 
        public virtual string PaymentReference {get;set;}  = ""; 
        public virtual string PayeePaymentReference {get;set;}  = ""; 
        public virtual string CallbackUrl {get;set;}  = ""; 
        public virtual string CallbackIdentifier {get;set;}  = ""; 
        public virtual string Status {get;set;}  = ""; 
        public virtual decimal Amount {get;set;} 
        public virtual string Currency {get;set;}  = ""; 
        public virtual string PayerAlias {get;set;}  = ""; 
        public virtual string PayeeAlias {get;set;}  = ""; 
        public virtual string Message {get;set;}  = ""; 
        public virtual string ErrorMessage {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? DateCreated {get;set;} 
        [DataType(DataType.Text)]
        public virtual DateTime? DatePaid {get;set;} 
        public virtual string ErrorCode {get;set;}  = ""; 
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 
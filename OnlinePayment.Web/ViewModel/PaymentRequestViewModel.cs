
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace OnlinePayment.Web.ViewModel
{
    public partial class PaymentRequestViewModel : ViewModelBase
    {
        public virtual string Session {get;set;}  = ""; 
        public virtual string PayeePaymentReference {get;set;}  = ""; 
        public virtual string CallbackUrl {get;set;}  = ""; 
        public virtual string PayerAlias {get;set;}  = ""; 
        public virtual string PayeeAlias {get;set;}  = ""; 
        public virtual int Amount {get;set;} 
        public virtual string Currency {get;set;}  = ""; 
        public virtual string Message {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? PaymentRequestDateTime {get;set;} 
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 
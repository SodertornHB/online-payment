
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;

namespace OnlinePayment.Logic.Model
{
    public partial class PaymentRequest : Entity
    {
        public virtual string Session { get; set; }
        public virtual string PayeePaymentReference {get;set;}
        public virtual string CallbackUrl {get;set;}
        public virtual string PayerAlias {get;set;}
        public virtual string PayeeAlias {get;set;}
        public virtual string Amount {get;set;}
        public virtual string Currency {get;set;}
        public virtual string Message {get;set; }
        public virtual DateTime? PaymentRequestDateTime { get; set; }

    }
} 

//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;

namespace OnlinePayment.Logic.Model
{
    public partial class PaymentCallback : Entity
    {
        public virtual string Session {get;set;}
        public virtual string PaymentReference {get;set;}
        public virtual string Status {get;set;}
        public virtual int Amount {get;set;}
        public virtual string Currency {get;set;}
        public virtual string PayerAlias {get;set;}
        public virtual string PayeeAlias {get;set;}
        public virtual DateTime? DatePaid {get;set;}
        public virtual string ErrorCode {get;set;}
      
    }
} 
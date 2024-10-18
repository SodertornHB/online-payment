
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;

namespace OnlinePayment.Logic.Model
{
    public partial class PaymentResponse : Entity
    {
        public virtual string Session {get;set;}
        public virtual string Location {get;set;}
        public virtual DateTime? PaymentResponseReceivedDateTime {get;set;}
        public virtual DateTime? CallbackReceivedDateTime {get;set;}
      
    }
} 
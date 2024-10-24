
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;

namespace OnlinePayment.Logic.Model
{
    public partial class Payment : Entity
    {
        public virtual string ExternalId {get;set;}
        public virtual string Session {get;set;}
        public virtual int BorrowerNumber {get;set;}
        public virtual string PatronName {get;set;}
        public virtual string PatronEmail {get;set;}
        public virtual string PatronPhoneNumber {get;set;}
        public virtual int Amount {get;set;}
        public virtual DateTime? InitiationDateTime {get;set;}
        public virtual string Status {get;set;}
        public virtual string Description {get;set;}
        public virtual string QrCode {get;set;}
      
    }
} 

//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace OnlinePayment.Web.ViewModel
{
    public partial class PaymentViewModel : ViewModelBase
    {
        public virtual string ExternalId {get;set;}  = ""; 
        public virtual string Session {get;set;}  = ""; 
        public virtual int BorrowerNumber {get;set;} 
        public virtual string PatronName {get;set;}  = ""; 
        public virtual string PatronEmail {get;set;}  = ""; 
        public virtual string PatronPhoneNumber {get;set;}  = ""; 
        public virtual int Amount {get;set;} 
        [DataType(DataType.Text)]
        public virtual DateTime? InitiationDateTime {get;set;} 
        public virtual string Status {get;set;}  = ""; 
        public virtual string Description {get;set;}  = ""; 
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 

//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace OnlinePayment.Web.ViewModel
{
    public partial class PaymentResponseViewModel : ViewModelBase
    {
        public virtual string Session {get;set;}  = ""; 
        public virtual string Location {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? PaymentResponseReceivedDateTime {get;set;} 
        [DataType(DataType.Text)]
        public virtual DateTime? CallbackReceivedDateTime {get;set;} 
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 
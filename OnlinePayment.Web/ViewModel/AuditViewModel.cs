
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;

namespace OnlinePayment.Web.ViewModel
{
    public partial class AuditViewModel : ViewModelBase
    {
        public virtual string Session {get;set;}  = ""; 
        public virtual string Message {get;set;}  = ""; 
        [DataType(DataType.Text)]
        public virtual DateTime? DateTime {get;set;} 
        public virtual string Entity {get;set;}  = ""; 
        public virtual string GetBackToListLink(string applicationName) => $"/{applicationName}/{GetType().Name.Replace("ViewModel","")}";
    }
} 
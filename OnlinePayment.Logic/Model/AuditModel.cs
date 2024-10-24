
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System;

namespace OnlinePayment.Logic.Model
{
    public partial class Audit : Entity
    {
        public Audit()
        {
        }
        public Audit(string message, string session, Type type)
        {
            this.Message = message;
            this.Session = session;
            this.Entity = type.Name;
            this.DateTime = System.DateTime.Now;
        }
        public virtual string Session { get; set; }
        public virtual string Message { get; set; }
        public virtual DateTime? DateTime { get; set; }
        public virtual string Entity { get; set; }

    }
}
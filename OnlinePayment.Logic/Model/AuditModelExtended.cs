using System;

namespace OnlinePayment.Logic.Model
{
    public partial class Audit
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
        public bool BelongsToSameSession(string session) => string.Compare(Session, session, true) == 0;

    }
} 
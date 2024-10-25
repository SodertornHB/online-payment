using System.Collections.Generic;

namespace OnlinePayment.Web.ViewModel
{
    public partial class SessionViewModel
    {
        public virtual IEnumerable<AuditViewModel> Audits {get;set;} = new List<AuditViewModel>();
    }
} 
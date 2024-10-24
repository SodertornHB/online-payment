using System.Collections.Generic;

namespace OnlinePayment.Web.ViewModel
{
    public partial class PayViewModel
    {
        public virtual IEnumerable<AuditViewModel> Audits {get;set;} = new List<AuditViewModel>();
    }
} 
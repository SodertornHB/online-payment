namespace OnlinePayment.Web.ViewModel
{
    public partial class AuditViewModel 
    {
        public string ToHtml()
        {
            return $"<span>{DateTime.Value} : {Message}</span>";
        }
    }
} 
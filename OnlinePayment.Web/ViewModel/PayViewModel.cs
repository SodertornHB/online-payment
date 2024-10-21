namespace OnlinePayment.Web.ViewModel
{
    public partial class PayViewModel 
    {
        public virtual int BorrowerNumber {get;set;} 
        public virtual string PatronName {get;set;}  = ""; 
        public virtual string PatronEmail {get;set;}  = ""; 
        public virtual string PatronPhoneNumber {get;set;}  = ""; 
        public virtual decimal Amount {get;set;} 
    }
} 
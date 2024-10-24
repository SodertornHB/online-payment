namespace OnlinePayment.Logic.Model
{
    public  class QrCodeRequestModel
    {
        public virtual string Format {get;set; }
        public virtual int Size { get; set; }
        public virtual string Token { get; set; }      
    }
} 
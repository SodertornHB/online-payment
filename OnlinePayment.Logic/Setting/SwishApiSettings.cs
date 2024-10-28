namespace OnlinePayment.Logic.Settings
{
    public class SwishApiSettings
    {
        public string Endpoint { get; set; }
        public string CallbackUrl { get; set; }
        public string PayeeAlias { get; set; }
        public string Currency { get; set; }
        public string Message { get; set; }
    }
}

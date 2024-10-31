using System.Linq;

namespace OnlinePayment.Web.ViewModel
{
    public class InitPayViewModel
    {
        public int BorrowerNumber { get; set; }
        public string PatronName { get; set; } = "";
        public string PatronEmail { get; set; } = "";
        public string PatronPhoneNumber { get; set; } = "";
        public string GetPatronPhoneNumber() => GetFormattedPhoneNumber();
        public int Amount { get; set; }
        public string GetAmountWithCurrency() => $"{Amount} SEK";
        public string Feedback { get; set; } = "";
        public bool HasFeedback()=>!string.IsNullOrEmpty(Feedback);

        private string GetFormattedPhoneNumber()
        {
            if (string.IsNullOrEmpty(PatronPhoneNumber))
            {
                return PatronPhoneNumber; 
            }

            string cleanedNumber = new string(PatronPhoneNumber.Where(char.IsDigit).ToArray());

            if (cleanedNumber.StartsWith("07") && cleanedNumber.Length == 10)
            {
                return string.Format("{0}-{1} {2} {3}",
                    cleanedNumber.Substring(0, 3),
                    cleanedNumber.Substring(3, 3),
                    cleanedNumber.Substring(6, 2),
                    cleanedNumber.Substring(8, 2));
            }
            else if (cleanedNumber.StartsWith("7") && cleanedNumber.Length == 9)
            {
                return string.Format("0{0}-{1} {2} {3}",
                    cleanedNumber.Substring(0, 2),
                    cleanedNumber.Substring(2, 3),
                    cleanedNumber.Substring(5, 2),
                    cleanedNumber.Substring(7, 2));
            }
            else if (cleanedNumber.Length == 11 && cleanedNumber.StartsWith("46"))
            {
                return string.Format("+46 {0} {1} {2} {3}",
                    cleanedNumber.Substring(2, 3),
                    cleanedNumber.Substring(5, 2),
                    cleanedNumber.Substring(7, 2),
                    cleanedNumber.Substring(9, 2));
            }

            return cleanedNumber;
        }

    }
} 
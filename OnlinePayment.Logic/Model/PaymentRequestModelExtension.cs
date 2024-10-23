
using System.Globalization;
using System;
using OnlinePayment.Logic.DataAccess;

namespace OnlinePayment.Logic.Model
{
    public partial class PaymentRequest
    {
        [SqlInsertIgnore]
        public string PaymentReference { get; set; }
        [SqlInsertIgnore]
        public virtual string Status { get; set; }

        public bool IsValid() => decimal.TryParse(Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal _);

        public void ConvertAmountToInt()
        {
            if (decimal.TryParse(Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedAmount))
            {
                Amount = ((int)Math.Round(parsedAmount)).ToString();
            }
            else
            {
                throw new FormatException("Amount must be a valid decimal number.");
            }
        }
    }
}
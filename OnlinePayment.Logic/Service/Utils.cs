using System;

namespace OnlinePayment.Logic.Services
{
    public static class Utils
    {
        public static int ConvertToInt(decimal amount)
        {
            if (amount > int.MaxValue || amount < int.MinValue)
            {
                throw new OverflowException("The amount is outside the range of an int.");
            }

            return (int)Math.Round(amount, MidpointRounding.AwayFromZero);
        }
    }
}

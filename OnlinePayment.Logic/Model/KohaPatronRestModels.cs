using OnlinePayment.Logic.Services;
using System;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace OnlinePayment.Logic.Model
{
    public class Patron
    {
        public int patron_id { get; set; }
        public string firstname { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string GetPhone() {

            if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentException("Patron lacking phone number");
            var formattedPhone = phone.Trim().Replace(" ","");
            if (formattedPhone.StartsWith('0'))
            {
                formattedPhone = formattedPhone.Substring(1);
                formattedPhone = $"46{formattedPhone}";
            }
            if (formattedPhone.StartsWith('+'))
            {
                formattedPhone = formattedPhone.Substring(1);
            }
            if (!formattedPhone.StartsWith("46")) throw new ArgumentException("Invalid phone number");
            if (!Regex.IsMatch(formattedPhone, @"^\d+$")) throw new ArgumentException("Phone number contains invalid characters");

            return formattedPhone;
        }
    }
    public class PatronAccount
    {
        public decimal balance { get; set; }

        public int GetBalance()
        {
            var b = Utils.ConvertToInt(balance);
            if (b == 0) throw new ArgumentException("Balance is zero");
            if (b < 0) throw new ArgumentException("Balance is less than zero");
            return b;
        }
    }
    public class PatronCredit
    {
        public decimal amount { get; set; }
        public string library_id { get; set; }
        public string credit_type { get; set; } = "swish";
        public string payment_type { get; set; } = "swish";
    }
} 
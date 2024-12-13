using OnlinePayment.Logic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public string GetPhone()
        {

            if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentException("Patron lacking phone number");
            var formattedPhone = phone.Trim().Replace(" ", "");
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
        public string GetFullname() => $"{firstname} {surname}";
    }
    public class PatronAccount
    {
        public decimal balance { get; set; }

        public outstanding_debits outstanding_debits { get; set; }

        public int GetBalance() => GetBalanceOrThrow(Utils.ConvertToInt(balance));

        public int GetBalanceForGivenStatuses(string[] statuses)
        {
            var sum = default(decimal);
            if (outstanding_debits == null) return default;
            foreach (var status in statuses)
            {
                    sum += outstanding_debits.lines.Where(x => x.status == null || x.status == status).Sum(x => x.amount);
             
            }
            return GetBalanceOrThrow(Utils.ConvertToInt(sum));
        }

        #region private

        private int GetBalanceOrThrow(int balance)
        {
            if (balance == 0) throw new ArgumentException("Balance is zero");
            if (balance < 0) throw new ArgumentException("Balance is less than zero");
            return balance;
        }

        #endregion
    }
    public class outstanding_debits
    {
        public decimal returned_balance => lines.Where(x => x.status == "RETURNED").Sum(x => x.amount);
        public IEnumerable<outstanding_debits_lines> lines { get; set; } = new List<outstanding_debits_lines>();
    }

    public class outstanding_debits_lines
    {
        public decimal amount { get; set; }
        public string status { get; set; }
    }

    public class PatronCredit
    {
        public decimal amount { get; set; }
        public string library_id { get; set; }
        public string credit_type { get; set; } = "swish";
        public string payment_type { get; set; } = "swish";
    }
}
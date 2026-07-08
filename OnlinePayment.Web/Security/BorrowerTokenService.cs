using Microsoft.AspNetCore.DataProtection;
using System;

namespace OnlinePayment.Web.Security
{
    /// <summary>
    /// Policy name for the rate limiter applied to the public payment entry points
    /// (/js, /init, /pay). Configured in StartupExtended.
    /// </summary>
    public static class PaymentRateLimit
    {
        public const string Policy = "payment-endpoints";
    }

    public interface IBorrowerTokenService
    {
        string Protect(int borrowerNumber);
        bool TryResolve(string token, out int borrowerNumber);
    }

    /// <summary>
    /// Issues short-lived, integrity-protected tokens that stand in for a borrower number on
    /// the public payment endpoints. This keeps a borrower from being addressed by guessing a
    /// sequential integer, and makes leaked links stop working once the token expires. Backed
    /// by ASP.NET Core Data Protection (time-limited protector).
    ///
    /// Note: the token is not a substitute for authentication. It cannot prove that the caller
    /// is the borrower — only that the identifier is unguessable and unexpired. See the
    /// SECURITY_AUDIT.md notes on finding 4 for the residual /js enumeration surface.
    /// </summary>
    public class BorrowerTokenService : IBorrowerTokenService
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);
        private readonly ITimeLimitedDataProtector protector;

        public BorrowerTokenService(IDataProtectionProvider provider)
        {
            protector = provider.CreateProtector("OnlinePayment.BorrowerToken").ToTimeLimitedDataProtector();
        }

        public string Protect(int borrowerNumber)
            => protector.Protect(borrowerNumber.ToString(), TokenLifetime);

        public bool TryResolve(string token, out int borrowerNumber)
        {
            borrowerNumber = 0;
            if (string.IsNullOrEmpty(token)) return false;
            try
            {
                return int.TryParse(protector.Unprotect(token), out borrowerNumber);
            }
            catch (Exception)
            {
                return false; // tampered, malformed, or expired
            }
        }
    }
}

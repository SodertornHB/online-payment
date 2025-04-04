using OnlinePayment.Logic.Model;
using System;
using NUnit.Framework;

namespace OnlinePayment.Test
{
    public partial class PatronModelTests
    {
        [TestCase("+46761336012", "46761336012")]
        [TestCase("46761336012", "46761336012")]
        [TestCase("+460761336012", "46761336012")]
        [TestCase("460761336012", "46761336012")]
        [TestCase("0761336012", "46761336012")]
        [TestCase("0761336012", "46761336012")]
        [TestCase("076-133 60 12", "46761336012")]
        [TestCase("076 133 60 12", "46761336012")]
        [TestCase("+46761336012", "46761336012")]
        [TestCase("0046761336012", "46761336012")]
        [TestCase("+46 76 133 60 12", "46761336012")]
        [TestCase("0046 76 133 6012", "46761336012")]
        [TestCase("+460761336012", "46761336012")]
        [TestCase("460761336012", "46761336012")]
        public void GetWashedPhoneNumber(string phoneNumber, string result)
        {
            var sut = new Patron { phone = phoneNumber };
            var washedPhoneNumber = sut.GetPhone();
            Assert.That(result, Is.EqualTo(washedPhoneNumber));
        }
    }
}

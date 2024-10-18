
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.Model;
using System;
using NUnit.Framework;

namespace OnlinePayment.Test
{    
    public partial class PaymentCallbackModelTests
    {
         [Test]
        public void Initiate_SholdNotBeNull()
        { 
                var id = 1;
                    var sut = new PaymentCallback { Id = id };            
            Assert.That(sut,!Is.Null);
            Assert.That(id, Is.EqualTo(sut.Id));
        }
        
                          
        [TestCase(18, 18, true)]
        [TestCase(18, 28, false)]
                public void CompairTwoEntities(int id, int compairId, bool result)
        {
            var sut = new PaymentCallback { Id = id };
            var compair = new PaymentCallback { Id = compairId };
            Assert.That(result, Is.EqualTo(sut.Equals(compair)));
        }
    }
}


//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using OnlinePayment.Logic.DataAccess;
using OnlinePayment.Logic.Model;
using OnlinePayment.Logic.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace OnlinePayment.Test
{    
    public class PaymentRequestServiceTests
    {
        private Mock<ILogger<PaymentRequestService>> loggerMock;
        private Mock<IPaymentRequestDataAccess> dataAccessMock;

        [SetUp]
        public void Setup()
        {
            loggerMock = new Mock<ILogger<PaymentRequestService>>();
            dataAccessMock = new Mock<IPaymentRequestDataAccess>();
        }

        [Test]
        public void Initiate_SholdNotBeNull()
        {
            var sut = new PaymentRequestService(loggerMock.Object, dataAccessMock.Object);
            Assert.That(sut,!Is.Null);
        }
        
        [Test]
        public async Task GetPaymentRequest_SholdNotBeNull()
        {
             
        var id3 = 3;
            dataAccessMock.Setup(x => x.Get(It.IsAny<int>())).Returns(Task.FromResult(new PaymentRequest { Id = id3 }));
            var sut = new PaymentRequestService(loggerMock.Object, dataAccessMock.Object);
            var result = await sut.Get(id3);
            Assert.That(sut, !Is.Null);
            Assert.That(id3, Is.EqualTo( result.Id));
            loggerMock.VerifyLoggingExact(LogLevel.Information, $"Fetching entity with id {id3} from data source.");
        }

        [Test]
        public async Task GetAllPaymentRequests_SholdContainEntities()
        {
             
            var id1 = 1;
            var id2 = 2;
            var id3 = 3;
            var testData = new List<PaymentRequest>
            {
                new PaymentRequest{ Id = id1 },
                new PaymentRequest{ Id = id2 },
                new PaymentRequest{ Id = id3 }
            };
            dataAccessMock.Setup(x => x.GetAll()).Returns(Task.FromResult(testData.AsEnumerable()));
            var sut = new PaymentRequestService(loggerMock.Object, dataAccessMock.Object);
            var result = await sut.GetAll();
            Assert.That(3, Is.EqualTo( result.Count()));
            Assert.That(id1, Is.EqualTo(result.First().Id));
            Assert.That(id3, Is.EqualTo(result.Last().Id));
            loggerMock.VerifyLoggingExact(LogLevel.Information, "Fetching all entities from data source.");
        }

        [Test]
        public async Task InsertPaymentRequest_VerifyInsertIsCalled()
        {
             
            var id1 = 1;
                    var sut = new PaymentRequestService(loggerMock.Object, dataAccessMock.Object);
            await sut.Insert(new PaymentRequest { Id = id1 });
            dataAccessMock.Verify(x => x.Insert(It.Is<PaymentRequest>(y => y.Id == id1)));
            loggerMock.VerifyLoggingContains(LogLevel.Information, "Saving entity");
        }

        [Test]
        public async Task UpdatePaymentRequest_VerifyUpdateIsCalled()
        {
             
        var id1 = 1;
            var sut = new PaymentRequestService(loggerMock.Object, dataAccessMock.Object);
            await sut.Update(new PaymentRequest { Id = id1 });
            dataAccessMock.Verify(x => x.Update(It.Is<PaymentRequest>(y => y.Id == id1)));
            loggerMock.VerifyLoggingContains(LogLevel.Information, "Update entity");
        }

        [Test]
        public async Task DeletePaymentRequest_VerifyDeleteIsCalled()
        {
             
            var id1 = 1;
            var sut = new PaymentRequestService(loggerMock.Object, dataAccessMock.Object);
            await sut.Delete(id1);
            dataAccessMock.Verify(x => x.Delete(It.Is<int>(y => y == id1)));
            loggerMock.VerifyLoggingExact(LogLevel.Information, $"Deleting entity with id {id1} from data source.");
        }
    }
}

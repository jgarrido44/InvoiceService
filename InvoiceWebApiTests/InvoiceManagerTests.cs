using InvoiceWebAPI;
using InvoiceWebAPI.Controllers;
using InvoiceWebAPI.InvoiceDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace InvoiceWebApiTests
{
    [TestFixture]
    internal class InvoiceManagerTests
    {
        private InvoiceManager _invoiceManager;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILogger<InvoiceController>> _loggerMock;
        private Mock<DbContextOptionsBuilder<InvoiceDbContext>> _optionsBuilderMock;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x.GetValue<string>("ConnectionString")).Returns("fakeConnectionString");

            _loggerMock = new Mock<ILogger<InvoiceController>>();
        }

        [Test]
        public void RetrieveInvoicesFromDb_ReturnsListOfInvoices()
        {
            //Arrange
            InvoiceManager invoiceManager = new(_configurationMock.Object, _loggerMock.Object);

            //Act
            var result = invoiceManager.RetrieveInvoicesFromDb();

            //Assert
            Assert.IsInstanceOf<List<Invoice>>(result);
        }
    }
}

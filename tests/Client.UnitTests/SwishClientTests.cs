using NSubstitute;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Client.UnitTests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class SwishClientTests
    {
        private readonly Payment _defaultPayment;
        private readonly IConfiguration configuration;

        public SwishClientTests()
        {
            _defaultPayment = new Payment()
            {
                Amount = "100",
                Currency = "SEK",
                CallbackUrl = "https://example.com/api/swishcb/paymentrequests",
                PayeeAlias = "1231181189",
                PayeePaymentReference = "0123456789",
                PayerAlias = "4671234768",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            configuration = Substitute.For<IConfiguration>();
            configuration.BaseUri().Returns(new Uri("https://mss.swicpc.bankgirot.se"));
        }

        [Fact]
        public void Test()
        {
            var clientCert = new X509Certificate2("testcertificates/SwishMerchantTestCertificate1231181189.p12", "swish");
            var caCert = new X509Certificate2("testcertificates/TestSwishRootCAv1Test.pem");
            SwishClient client = new SwishClient(configuration, clientCert, caCert);
            client.MakePayment(_defaultPayment);
        }

        [Fact]
        public void Throws_Swich_Exception_When_Status_Code_Is_422()
        {

            var errorMsg = "Testing error";
            var mockHttp = MockHttp.WithStatusAndContent(422, errorMsg);
            var client = new SwishClient(mockHttp);
            var exception = Assert.Throws<SwishException>(() => client.MakePayment(_defaultPayment));
            Assert.Equal(errorMsg, exception.Message);
        }

        [Fact]
        public void Throws_Http_Exception_For_Not_Ok_Status_Codes()
        {
            var mockHttp = MockHttp.WithStatus(500);
            var client = new SwishClient(mockHttp);
            Assert.Throws<HttpRequestException>(() => client.MakePayment(_defaultPayment));
        }
    }
}

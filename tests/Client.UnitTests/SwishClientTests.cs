using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Client.UnitTests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class SwishClientTests
    {
        private readonly PaymentModel _defaultPayment;
        private readonly IConfiguration configuration;
        private readonly RefundModel _defaultRefund;

        public SwishClientTests()
        {
            _defaultPayment = new PaymentModel()
            {
                Amount = "100",
                Currency = "SEK",
                CallbackUrl = "https://example.com/api/swishcb/paymentrequests",
                PayeeAlias = "1231181189",
                PayeePaymentReference = "0123456789",
                PayerAlias = "4671234768",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            _defaultRefund = new RefundModel()
            {
                PayerPaymentReference = "0123456789",
                OriginalPaymentReferences = "6D6CD7406ECE4542A80152D909EF9F6B",
                CallbackUrl = "https://example.com/api/swishcb/refunds%22",
                PayerAlias = "4671234768",
                PayeeAlias = "1231181189",
                Amount = "100",
                Currency = "SEK",
                Message = "Refund for Kingston USB Flash Drive 8 GB"
            };

            configuration = Substitute.For<IConfiguration>();
            configuration.BaseUri().Returns(new Uri("https://mss.swicpc.bankgirot.se"));
        }

        [Fact]
        public async Task Test()
        {
            var clientCert = new X509Certificate2("testcertificates/SwishMerchantTestCertificate1231181189.p12", "swish");
            var caCert = new X509Certificate2("testcertificates/TestSwishRootCAv1Test.pem");
            SwishClient client = new SwishClient(configuration, clientCert, caCert);
            await client.MakePaymentAsync(_defaultPayment);
        }

        [Fact]
        public async Task Test2()
        {
            var clientCert = new X509Certificate2("testcertificates/SwishMerchantTestCertificate1231181189.p12", "swish");
            var caCert = new X509Certificate2("testcertificates/TestSwishRootCAv1Test.pem");
            SwishClient client = new SwishClient(configuration, clientCert, caCert);
            await client.MakeRefundAsync(_defaultRefund);

        }

        [Fact]
        public async Task MakePayment_Returns_Location_Header_Values()
        {
            string paymentId = "AB23D7406ECE4542A80152D909EF9F6B";
            string locationHeader = $"https://mss.swicpc.bankgirot.se/swishcpcapi/v1/paymentrequests/{paymentId}";
            var headerValues = new Dictionary<string, string>() {{"Location", locationHeader}};
            var responseMessage = Create201HttpJsonResponseMessage(_defaultPayment, headerValues);
            var client = new SwishClient(MockHttp.WithResponseMessage(responseMessage));

            // Act
            var response = await client.MakePaymentAsync(_defaultPayment);

            // Assert
            Assert.Equal(response.Location, locationHeader);
            Assert.Equal(response.Id, paymentId);
        }

        [Fact]
        public async Task MakeRefund_Returns_Location_Header_Values()
        {
            string refundId = "ABC2D7406ECE4542A80152D909EF9F6B";
            string locationHeader = $"https://mss.swicpc.bankgirot.se/swishcpcapi/v1/refunds/{refundId}";
            var headerValues = new Dictionary<string, string>() { { "Location", locationHeader } };
            var responseMessage = Create201HttpJsonResponseMessage(_defaultPayment, headerValues);
            var client = new SwishClient(MockHttp.WithResponseMessage(responseMessage));

            // Act
            var response = await client.MakeRefundAsync(_defaultRefund);

            // Assert
            Assert.Equal(response.Location, locationHeader);
            Assert.Equal(response.Id, refundId);
        }

        private HttpResponseMessage Create201HttpJsonResponseMessage<T>(T contentModel,
            Dictionary<string, string> headerValues)
        {
            var content = new StringContent(JsonConvert.SerializeObject(_defaultPayment), Encoding.UTF8, "application/json");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Created) { Content = content };
            foreach (var header in headerValues)
            {
                responseMessage.Headers.Add(header.Key, header.Value);
            }
            return responseMessage;
        }

        [Fact]
        public async Task Throws_Swich_Exception_When_Status_Code_Is_422()
        {
            var errorMsg = "Testing error";
            var mockHttp = MockHttp.WithStatusAndContent(422, errorMsg);
            var client = new SwishClient(mockHttp);
            var exception = await Assert.ThrowsAsync<SwishException>(() => client.MakePaymentAsync(_defaultPayment));
            Assert.Equal(errorMsg, exception.Message);
        }

        [Fact]
        public async Task Throws_Http_Exception_For_Not_Ok_Status_Codes()
        {
            var mockHttp = MockHttp.WithStatus(500);
            var client = new SwishClient(mockHttp);
            await Assert.ThrowsAsync<HttpRequestException>(() => client.MakePaymentAsync(_defaultPayment));
        }
    }
}

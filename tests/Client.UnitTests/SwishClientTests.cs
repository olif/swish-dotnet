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
        private readonly ECommercePaymentModel _defaultECommercePaymentModel;
        private readonly MCommercePaymentModel _defaultMCommercePaymentModel;
        private readonly RefundModel _defaultRefund;

        public SwishClientTests()
        {
            _defaultECommercePaymentModel = new ECommercePaymentModel()
            {
                Amount = "100",
                Currency = "SEK",
                CallbackUrl = "https://example.com/api/swishcb/paymentrequests",
                PayerAlias = "467012345678",
                PayeeAlias = "1231181189",
                PayeePaymentReference = "0123456789",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            _defaultMCommercePaymentModel = new MCommercePaymentModel()
            {
                Amount = "100",
                Currency = "SEK",
                CallbackUrl = "https://example.com/api/swishcb/paymentrequests",
                PayeeAlias = "1231181189",
                PayeePaymentReference = "0123456789",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            _defaultRefund = new RefundModel()
            {
                PayerPaymentReference = "0123456789",
                OriginalPaymentReference = "6D6CD7406ECE4542A80152D909EF9F6B",
                CallbackUrl = "https://example.com/api/swishcb/refunds%22",
                PayerAlias = "4671234768",
                PayeeAlias = "1231181189",
                Amount = "100",
                Currency = "SEK",
                Message = "Refund for Kingston USB Flash Drive 8 GB"
            };
        }

        /*[Fact]
        public async Task Test()
        {
            var clientCert = new X509Certificate2("testcertificates/SwishMerchantTestCertificate1231181189.p12", "swish");
            var caCert = new X509Certificate2("testcertificates/TestSwishRootCAv1Test.pem");
            SwishClient client = new SwishClient(configuration, clientCert, caCert);
            await client.MakePaymentAsync(_defaultPayment);
        }*/

        /*[Fact]
        public async Task Test2()
        {
            var clientCert = new X509Certificate2("testcertificates/SwishMerchantTestCertificate1231181189.p12", "swish");
            var caCert = new X509Certificate2("testcertificates/TestSwishRootCAv1Test.pem");
            SwishClient client = new SwishClient(configuration, clientCert, caCert);
            await client.MakeRefundAsync(_defaultRefund);
        }*/

        [Fact]
        public async Task MakeECommercePayment_Returns_Location_Header_Values()
        {
            string paymentId = "AB23D7406ECE4542A80152D909EF9F6B";
            string locationHeader = $"https://mss.swicpc.bankgirot.se/swishcpcapi/v1/paymentrequests/{paymentId}";
            var headerValues = new Dictionary<string, string>() { { "Location", locationHeader } };
            var responseMessage = Create201HttpJsonResponseMessage(_defaultECommercePaymentModel, headerValues);
            var client = new SwishClient(MockHttp.WithResponseMessage(responseMessage));

            // Act
            var response = await client.MakeECommercePaymentAsync(_defaultECommercePaymentModel);

            // Assert
            Assert.Equal(response.Location, locationHeader);
            Assert.Equal(response.Id, paymentId);
        }

        [Fact]
        public async Task MakeECommercePayment_Throws_Swich_Exception_When_Status_Code_Is_422()
        {
            var errorMsg = "Testing error";
            var mockHttp = MockHttp.WithStatusAndContent(422, errorMsg);
            var client = new SwishClient(mockHttp);
            var exception = await Assert.ThrowsAsync<SwishException>(() => 
                client.MakeECommercePaymentAsync(_defaultECommercePaymentModel));
            Assert.Equal(errorMsg, exception.Message);
        }

        [Fact]
        public async Task MakeECommercePayment_Throws_Http_Exception_For_Not_Ok_Status_Codes()
        {
            var mockHttp = MockHttp.WithStatus(500);
            var client = new SwishClient(mockHttp);
            await Assert.ThrowsAsync<HttpRequestException>(() => 
            client.MakeECommercePaymentAsync(_defaultECommercePaymentModel));
        }

        [Fact]
        public async Task MakeMCommercePayment_Returns_Location_And_Token_Header_VaLues()
        {
            string paymentId = "AB23D7406ECE4542A80152D909EF9F6B";
            string locationHeader = $"https://mss.swicpc.bankgirot.se/swishcpcapi/v1/paymentrequests/{paymentId}";
            var headerValues = new Dictionary<string, string>()
            {
                { "Location", locationHeader },
                { "PaymentRequestToken", "f34DS34lfd0d03fdDselkfd3ffk21" }
            };
            var responseMessage = Create201HttpJsonResponseMessage(_defaultMCommercePaymentModel, headerValues);
            var client = new SwishClient(MockHttp.WithResponseMessage(responseMessage));

            // Act
            var response = await client.MakeMCommercePaymentAsync(_defaultMCommercePaymentModel);

            // Assert
            Assert.Equal(response.Location, locationHeader);
            Assert.Equal(response.Id, paymentId);
            Assert.Equal(response.Token, "f34DS34lfd0d03fdDselkfd3ffk21");
        }

        [Fact]
        public async Task MakeMCommercePayment_Throws_Swich_Exception_When_Status_Code_Is_422()
        {
            var errorMsg = "Testing error";
            var mockHttp = MockHttp.WithStatusAndContent(422, errorMsg);
            var client = new SwishClient(mockHttp);
            var exception = await Assert.ThrowsAsync<SwishException>(() =>
                client.MakeMCommercePaymentAsync(_defaultMCommercePaymentModel));
            Assert.Equal(errorMsg, exception.Message);
        }

        [Fact]
        public async Task MakeMCommercePayment_Throws_Http_Exception_For_Not_Ok_Status_Codes()
        {
            var mockHttp = MockHttp.WithStatus(500);
            var client = new SwishClient(mockHttp);
            await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.MakeMCommercePaymentAsync(_defaultMCommercePaymentModel));
        }

        [Fact]
        public async Task MakeRefund_Returns_Location_Header_Values()
        {
            string refundId = "ABC2D7406ECE4542A80152D909EF9F6B";
            string locationHeader = $"https://mss.swicpc.bankgirot.se/swishcpcapi/v1/refunds/{refundId}";
            var headerValues = new Dictionary<string, string>() { { "Location", locationHeader } };
            var responseMessage = Create201HttpJsonResponseMessage(_defaultRefund, headerValues);
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
            var content = new StringContent(JsonConvert.SerializeObject(contentModel), Encoding.UTF8, "application/json");
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Created) { Content = content };
            foreach (var header in headerValues)
            {
                responseMessage.Headers.Add(header.Key, header.Value);
            }
            return responseMessage;
        }


    }
}

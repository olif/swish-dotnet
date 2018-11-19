using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
        private readonly IConfiguration _configuration;
        private readonly ECommercePaymentModel _defaultECommercePaymentModel;
        private readonly MCommercePaymentModel _defaultMCommercePaymentModel;
        private readonly RefundModel _defaultRefund;

        public SwishClientTests()
        {
            _configuration = new TestConfig("1231181189");
            _defaultECommercePaymentModel = new ECommercePaymentModel(
                amount: "100",
                callbackUrl: "https://example.com/api/swishcb/paymentrequests",
                currency: "SEK",
                payerAlias: "467012345678")
            {
                PayeePaymentReference = "0123456789",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            _defaultMCommercePaymentModel = new MCommercePaymentModel(
                amount: "100",
                callbackUrl: "https://example.com/api/swishcb/paymentrequests",
                currency: "SEK")
            {
                PayeePaymentReference = "0123456789",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            _defaultRefund = new RefundModel(
                originalPaymentReference: "6D6CD7406ECE4542A80152D909EF9F6B",
                callbackUrl: "https://example.com/api/swishcb/refunds",
                payerAlias: "1231181189",
                amount: "100",
                currency: "SEK")
            {
                PayerPaymentReference = "0123456789",
                Message = "Refund for Kingston USB Flash Drive 8 GB"
            };
        }

        [Fact]
        public async Task MakeECommercePayment_Returns_Location_Header_Values()
        {
            string paymentId = "AB23D7406ECE4542A80152D909EF9F6B";
            string locationHeader = $"https://mss.swicpc.bankgirot.se/swishcpcapi/v1/paymentrequests/{paymentId}";
            var headerValues = new Dictionary<string, string>() { { "Location", locationHeader } };
            var responseMessage = Create201HttpJsonResponseMessage(_defaultECommercePaymentModel, headerValues);
            var client = new SwishClient(_configuration, MockHttp.WithResponseMessage(responseMessage));

            // Act
            var response = await client.MakeECommercePaymentAsync(_defaultECommercePaymentModel);

            // Assert
            Assert.Equal(locationHeader, response.Location);
            Assert.Equal(paymentId, response.Id);
        }

        [Fact]
        public async Task MakeECommercePayment_Throws_Swich_Exception_When_Status_Code_Is_422()
        {
            var errorMsg = "Testing error";
            var mockHttp = MockHttp.WithStatusAndContent(422, errorMsg);
            var client = new SwishClient(_configuration, mockHttp);
            var exception = await Assert.ThrowsAsync<SwishException>(() => 
                client.MakeECommercePaymentAsync(_defaultECommercePaymentModel));
            Assert.Equal(errorMsg, exception.Message);
        }

        [Fact]
        public async Task MakeECommercePayment_Throws_Http_Exception_For_Not_Ok_Status_Codes()
        {
            var mockHttp = MockHttp.WithStatus(500);
            var client = new SwishClient(_configuration, mockHttp);
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
            var client = new SwishClient(_configuration, MockHttp.WithResponseMessage(responseMessage));

            // Act
            var response = await client.MakeMCommercePaymentAsync(_defaultMCommercePaymentModel);

            // Assert
            Assert.Equal(locationHeader, response.Location);
            Assert.Equal(paymentId, response.Id);
            Assert.Equal("f34DS34lfd0d03fdDselkfd3ffk21", response.Token);
        }

        [Fact]
        public async Task MakeMCommercePayment_Throws_Swich_Exception_When_Status_Code_Is_422()
        {
            var errorMsg = "Testing error";
            var mockHttp = MockHttp.WithStatusAndContent(422, errorMsg);
            var client = new SwishClient(_configuration, mockHttp);
            var exception = await Assert.ThrowsAsync<SwishException>(() =>
                client.MakeMCommercePaymentAsync(_defaultMCommercePaymentModel));
            Assert.Equal(errorMsg, exception.Message);
        }

        [Fact]
        public async Task MakeMCommercePayment_Throws_Http_Exception_For_Not_Ok_Status_Codes()
        {
            var mockHttp = MockHttp.WithStatus(500);
            var client = new SwishClient(_configuration, mockHttp);
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
            var client = new SwishClient(_configuration, MockHttp.WithResponseMessage(responseMessage));

            // Act
            var response = await client.MakeRefundAsync(_defaultRefund);

            // Assert
            Assert.Equal(locationHeader, response.Location);
            Assert.Equal(refundId, response.Id);
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

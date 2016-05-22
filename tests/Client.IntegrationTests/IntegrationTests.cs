using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Client.IntegrationTests
{
    public class IntegrationTests
    {
        private readonly IConfiguration configuration;

        public IntegrationTests()
        {

            configuration = Substitute.For<IConfiguration>();
            configuration.BaseUri().Returns(new Uri("https://mss.swicpc.bankgirot.se"));
        }

        [Fact]
        public async Task ECommerceScenario()
        {
            var clientCert = new X509Certificate2("testcertificates/SwishMerchantTestCertificate1231181189.p12", "swish");
            var caCert = new X509Certificate2("testcertificates/TestSwishRootCAv1Test.pem");
            var client = new SwishClient(configuration, clientCert, caCert);

            // Make payment
            var ecommercePaymentModel = new ECommercePaymentModel()
            {
                Amount = "100",
                Currency = "SEK",
                CallbackUrl = "https://example.com/api/swishcb/paymentrequests",
                PayerAlias = "1231234567890",
                PayeeAlias = "1231181189",
                PayeePaymentReference = "0123456789",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            var paymentResponse = await client.MakeECommercePaymentAsync(ecommercePaymentModel);

            // Wait so that the payment request has been processed
            Thread.Sleep(5000);

            // Check payment request status
            var paymentStatus = await client.GetPaymentStatus(paymentResponse.Id);
            Assert.Equal("PAID", paymentStatus.Status);

            // Make refund
            var refundModel = new RefundModel()
            {
                PayerPaymentReference = "0123456789",
                OriginalPaymentReference = paymentStatus.PaymentReference,
                CallbackUrl = "https://example.com/api/swishcb/refunds",
                PayerAlias = "1231181189",
                PayeeAlias = "1231234567890",
                Amount = "100",
                Currency = "SEK",
                Message = "Refund for Kingston USB Flash Drive 8 GB"
            };
            var refundResponse = await client.MakeRefundAsync(refundModel);

            // Wait so that the refund request has been processed
            Thread.Sleep(10000);

            // Check refund request status
            var refundStatus = await client.GetRefundStatus(refundResponse.Id);
            Assert.Equal("PAID", refundStatus.Status);
        }

        [Fact]
        public async Task MCommerceScenario()
        {
            var clientCert = new X509Certificate2("testcertificates/SwishMerchantTestCertificate1231181189.p12", "swish");
            var caCert = new X509Certificate2("testcertificates/TestSwishRootCAv1Test.pem");
            var client = new SwishClient(configuration, clientCert, caCert);

            // Make payment
            var mcommercePaymentModel = new MCommercePaymentModel()
            {
                Amount = "100",
                Currency = "SEK",
                CallbackUrl = "https://example.com/api/swishcb/paymentrequests",
                PayeeAlias = "1231181189",
                PayeePaymentReference = "0123456789",
                Message = "Kingston USB Flash Drive 8 GB"
            };

            var paymentResponse = await client.MakeMCommercePaymentAsync(mcommercePaymentModel);

            // Wait so that the payment request has been processed
            Thread.Sleep(5000);

            // Check payment request status
            var paymentStatus = await client.GetPaymentStatus(paymentResponse.Id);
            Assert.Equal("PAID", paymentStatus.Status);

            // Make refund
            var refundModel = new RefundModel()
            {
                PayerPaymentReference = "0123456789",
                OriginalPaymentReference = paymentStatus.PaymentReference,
                CallbackUrl = "https://example.com/api/swishcb/refunds",
                PayerAlias = "1231181189",
                PayeeAlias = "1231234567890",
                Amount = "100",
                Currency = "SEK",
                Message = "Refund for Kingston USB Flash Drive 8 GB"
            };
            var refundResponse = await client.MakeRefundAsync(refundModel);

            // Wait so that the refund request has been processed
            Thread.Sleep(10000);

            // Check refund request status
            var refundStatus = await client.GetRefundStatus(refundResponse.Id);
            Assert.Equal("PAID", refundStatus.Status);
        }
    }
}

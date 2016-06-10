# A .Net Swish Client

[![Build status](https://ci.appveyor.com/api/projects/status/xjbl5h0g5ccjhb60/branch/master?svg=true)](https://ci.appveyor.com/project/Olif/swish-dotnet/branch/master)


---

A .net swish client written in .net core rc2 that targets .net >= 4.5.1.

## Usage

### Initializing the client
    var clientCert = new X509Certificate2("testcertificates/SwishMerchantTestCertificate1231181189.p12", "swish");
    var caCert = new X509Certificate2("testcertificates/TestSwishRootCAv1Test.pem");
    var client = new SwishClient(configuration, clientCert, caCert);

### Making a payment request
    // Make payment
    var ecommercePaymentModel = new ECommercePaymentModel(
        amount: "100",
        currency: "SEK",
        callbackUrl: "https://example.com/api/swishcb/paymentrequests",
        payerAlias: "1231234567890")
    {
        PayeePaymentReference = "0123456789",
        Message = "Kingston USB Flash Drive 8 GB"
    };

    var paymentResponse = await client.MakeECommercePaymentAsync(ecommercePaymentModel);

    // Wait so that the payment request has been processed
    Thread.Sleep(5000);

    // Check payment request status
    var paymentStatus = await client.GetPaymentStatus(paymentResponse.Id);

### Making a refund request
    // Make refund
    var refundModel = new RefundModel(
        originalPaymentReference: paymentStatus.PaymentReference,
        callbackUrl: "https://example.com/api/swishcb/refunds",
        payerAlias: "1231181189",
        amount: "100",
        currency: "SEK")
    {
        PayerPaymentReference = "0123456789",
        Message = "Refund for Kingston USB Flash Drive 8 GB"
    };
    var refundResponse = await client.MakeRefundAsync(refundModel);

    // Wait so that the refund request has been processed
    Thread.Sleep(10000);

    // Check refund request status
    var refundStatus = await client.GetRefundStatus(refundResponse.Id);

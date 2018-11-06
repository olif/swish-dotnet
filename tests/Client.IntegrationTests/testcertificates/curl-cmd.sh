curl -v --header "Content-Type: application/json" \
    --request POST \
    --cert 'SwishMerchantTestCertificate1231181189.p12:swish' \
    --cert-type p12 \
    --tlsv1.1 \
    --data '{ "payeePaymentReference": "0123456789", "callbackUrl": "https://example.com/api/swishcb/paymentrequests", "payerAlias": "4671234768", "payeeAlias": "1231181189", "amount": "100", "currency": "SEK", "message": "Kingston USB Flash Drive 8 GB" }' \
https://mss.cpc.getswish.net/swish-cpcapi/api/v1/paymentrequests \


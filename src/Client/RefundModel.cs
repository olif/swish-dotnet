using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client
{
    public class RefundModel
    {
        public RefundModel(string originalPaymentReference, string callbackUrl, string payerAlias,
            string amount, string currency)
        {
            OriginalPaymentReference = originalPaymentReference;
            CallbackUrl = callbackUrl;
            PayerAlias = payerAlias;
            Amount = amount;
            Currency = currency;
        }

        public string PayerPaymentReference { get; set; }

        public string OriginalPaymentReference { get; protected set; }

        public string PaymentReference { get; set; }

        public string CallbackUrl { get; protected set; }

        public string PayerAlias { get; protected set; }

        public string PayeeAlias { get; protected set; }

        public string Amount { get; protected set; }

        public string Currency { get; protected set; }

        public string Message { get; set; }
    }

    public class RefundStatusModel : RefundModel
    {
        public RefundStatusModel(string originalPaymentReference, string callbackUrl, string payerAlias,
    string amount, string currency) : base(originalPaymentReference, callbackUrl, payerAlias, amount, currency)
        {}

        public string Status { get; set; }
    }
}

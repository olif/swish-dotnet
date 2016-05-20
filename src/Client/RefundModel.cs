using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client
{
    public class RefundModel
    {
        public string PayerPaymentReference { get; set; }

        public string OriginalPaymentReference { get; set; }

        public string PaymentReference { get; set; }

        public string CallbackUrl { get; set; }

        public string PayerAlias { get; set; }

        public string PayeeAlias { get; set; }

        public string Amount { get; set; }

        public string Currency { get; set; }

        public string Message { get; set; }
    }
}

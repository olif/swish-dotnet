using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client
{
    public class Payment
    {
        public string PayeePaymentReference { get; set; }

        public string CallbackUrl { get; set; }

        public string PayerAlias { get; set; }

        public string PayeeAlias { get; set; }

        public string Amount { get; set; }

        public string Currency { get; set; }

        public string Message { get; set; }
    }
}

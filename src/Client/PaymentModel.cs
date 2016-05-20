using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client
{
    public class PaymentModel
    {
        public string PayeePaymentReference { get; set; }

        public string CallbackUrl { get; set; }

        public string PayeeAlias { get; set; }

        public string Amount { get; set; }

        public string Currency { get; set; }

        public string Message { get; set; }
    }

    public class MCommercePaymentModel : PaymentModel
    {
        
    }

    public class ECommercePaymentModel : PaymentModel
    {
        public string PayerAlias { get; set; }
    }

    public class PaymentStatusModel : PaymentModel
    {
        public string Id { get; set; }

        public string PaymentReference { get; set; }

        public string Status { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DatePaid { get; set; }

        public string ErrorMessage { get; set; }

        public string AdditionalInformation { get; set; }
    }
}

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Client
{
    /// <summary>
    /// Swish client
    /// </summary>
    public class SwishClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private const string PaymentPath = "swish-cpcapi/api/v1/paymentrequests/";
        private const string RefundPath = "swish-cpcapi/api/v1/refunds";

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="configuration">The client configuration</param>
        /// <param name="cert">The client certificate</param>
        /// <param name="caCert">Optional CA root certificate used to verify server certificate, if not provided, no server certificate validation will be done</param>
        public SwishClient(IConfiguration configuration, X509Certificate2 cert, X509Certificate2 caCert = null)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);
            if (caCert != null)
            {
                SetupServerCertificateValidation(handler, caCert);
            }

            _configuration = configuration;
            _client = new HttpClient(handler) { BaseAddress = configuration.BaseUri() };
        }

        /// <summary>
        /// Initializes the swish client to use injected httpclient. Primarily used for testing purposes.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="httpClient">A HttpClient</param>
        public SwishClient(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _client = httpClient;
        }

        /// <summary>
        /// Makes a swish payment via the e-commerce flow
        /// </summary>
        /// <param name="payment">The payment details</param>
        /// <returns>Payment response containing payment status location</returns>
        public async Task<ECommercePaymentResponse> MakeECommercePaymentAsync(ECommercePaymentModel payment)
        {
            payment.PayeeAlias = _configuration.GetMerchantId();
            var response = await Post(payment, PaymentPath);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == (HttpStatusCode)422)
            {
                throw new SwishException(responseContent);
            }
            response.EnsureSuccessStatusCode();

            return ExtractSwishResponse(response) as ECommercePaymentResponse;
        }

        /// <summary>
        /// Make a swish payment via the m-commerce flow
        /// </summary>
        /// <param name="payment">The payment details</param>
        /// <returns>Payment response containing payment status location</returns>
        public async Task<MCommercePaymentResponse> MakeMCommercePaymentAsync(MCommercePaymentModel payment)
        {
            payment.PayeeAlias = _configuration.GetMerchantId();
            var response = await Post(payment, PaymentPath);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == (HttpStatusCode)422)
            {
                throw new SwishException(responseContent);
            }
            response.EnsureSuccessStatusCode();

            return ExtractMCommerceResponse(response);
        }

        /// <summary>
        /// Get the current status of a payment
        /// </summary>
        /// <param name="id">The location id</param>
        /// <returns>The payment status</returns>
        public async Task<PaymentStatusModel> GetPaymentStatus(string id)
        {
            var uri = $"{PaymentPath}/{id}";
            var response = await Get(uri);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<PaymentStatusModel>(responseContent);
        }

        /// <summary>
        /// Makes a refund request
        /// </summary>
        /// <param name="refund">The refund details</param>
        /// <returns>The refund response containing the location of the refund status</returns>
        public async Task<SwishApiResponse> MakeRefundAsync(RefundModel refund)
        {
            var response = await Post(refund, RefundPath);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == (HttpStatusCode)422)
            {
                throw new SwishException(responseContent);
            }
            response.EnsureSuccessStatusCode();

            return ExtractSwishResponse(response);
        }

        /// <summary>
        /// Get the current status of a refund
        /// </summary>
        /// <param name="id">The refund location id</param>
        /// <returns>The refund status</returns>
        public async Task<RefundStatusModel> GetRefundStatus(string id)
        {
            var uri = $"{RefundPath}/{id}";
            var response = await Get(uri);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<RefundStatusModel>(responseContent);
        }

        private SwishApiResponse ExtractSwishResponse(HttpResponseMessage responseMessage)
        {
            var location = responseMessage.Headers.GetValues("Location").FirstOrDefault();
            var swishResponse = new ECommercePaymentResponse();
            if (location != null)
            {
                var id = location.Split('/').LastOrDefault();
                swishResponse.Location = location;
                swishResponse.Id = id;
            }

            return swishResponse;
        }

        private MCommercePaymentResponse ExtractMCommerceResponse(HttpResponseMessage responseMessage)
        {
            var token = responseMessage.Headers.GetValues("PaymentRequestToken").FirstOrDefault();
            var paymentResponse = ExtractSwishResponse(responseMessage);

            return new MCommercePaymentResponse()
            {
                Id = paymentResponse.Id,
                Location = paymentResponse.Location,
                Token = token
            };
        }

        private Task<HttpResponseMessage> Post<T>(T model, string path)
        {
            var json = JsonConvert.SerializeObject(model, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = _client.PostAsync(path, content);

            return response;
        }

        private Task<HttpResponseMessage> Get(string path) => _client.GetAsync(path);

        private static void SetupServerCertificateValidation(HttpClientHandler handler, X509Certificate2 caCert)
        {
            handler.ServerCertificateCustomValidationCallback =
                (sender, certificate, chain, errors) =>
                {
                    var x509ChainElement = chain.ChainElements.OfType<X509ChainElement>().LastOrDefault();
                    if (x509ChainElement == null) return false;
                    var c = x509ChainElement.Certificate;
                    return c.Equals(caCert);
                };
        }
    }
}

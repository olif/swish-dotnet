using System;

namespace Client
{
    public interface IConfiguration
    {
        Uri BaseUri();

        string GetMerchantId();
    }

    public class ProductionConfig : IConfiguration
    {
        private readonly string _merchantId;
        public ProductionConfig(string merchantId)
        {
            _merchantId = merchantId;
        }

        public Uri BaseUri() => new Uri("https://swicpc.bankgirot.se");

        public string GetMerchantId() => _merchantId;
    }

    public class TestConfig : IConfiguration
    {
        private readonly string _merchantId;

        public TestConfig(string merchantId)
        {
            _merchantId = merchantId;
        }

        public Uri BaseUri() => new Uri("https://mss.swicpc.bankgirot.se");

        public string GetMerchantId() => _merchantId;
    }
}

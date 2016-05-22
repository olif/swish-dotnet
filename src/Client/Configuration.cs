using System;

namespace Client
{
    public interface IConfiguration
    {
        Uri BaseUri();
    }

    internal class ProductionConfig : IConfiguration
    {
        public Uri BaseUri() => new Uri("https://swicpc.bankgirot.se");
    }
}

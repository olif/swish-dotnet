using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    public class SwishException : HttpRequestException
    {
        public SwishException(string message) : base(message)
        { }
    }
}

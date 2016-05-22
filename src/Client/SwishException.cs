using System.Net.Http;

namespace Client
{
    public class SwishException : HttpRequestException
    {
        public SwishException(string message) : base(message)
        { }
    }
}

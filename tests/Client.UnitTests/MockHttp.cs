using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.UnitTests
{
    public class MockHttp : HttpClient
    {
        private readonly TestHttpMessageHandler _handler;

        public MockHttp(TestHttpMessageHandler handler) : base(handler)
        {
            _handler = handler;
            this.BaseAddress = new Uri("http://test");
        }

        public HttpRequestMessage LastRequest => _handler.Request;

        public static MockHttp WithStatusAndContent(int code, string content)
        {
            return new MockHttp(new TestHttpMessageHandler((HttpStatusCode)code, content));
        }

        public static MockHttp WithStatus(int status)
        {
            return new MockHttp(new TestHttpMessageHandler((HttpStatusCode)status));
        }
    }

    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _responseMessage;

        public TestHttpMessageHandler(HttpResponseMessage message)
        {
            _responseMessage = message;
        }

        public TestHttpMessageHandler(HttpStatusCode status)
            :this(CreateResponseMessage(status))
        {}

        public TestHttpMessageHandler(HttpStatusCode status, string content)
            : this(CreateJsonResponseMessage(status, content))
        {}

        public HttpRequestMessage Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Request = request;
            return Task.FromResult(_responseMessage);
        }

        private static HttpResponseMessage CreateResponseMessage(HttpStatusCode status)
        {
            var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
            return new HttpResponseMessage(status) {Content = emptyContent};
        }

        private static HttpResponseMessage CreateJsonResponseMessage(HttpStatusCode status, string content)
        {
            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            return new HttpResponseMessage(status) {Content = stringContent};
        }
    }
}

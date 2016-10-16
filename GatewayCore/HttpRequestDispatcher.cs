using Microsoft.ServiceFabric.Services.Communication.Client;
using System.Fabric;
using System.Net.Http;

namespace Iridium.GatewayCore
{
    
    /// <summary>
    /// Dispatcher of HTTP requests.
    /// </summary>
    public class HttpRequestDispatcher : HttpClient, ICommunicationClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestDispatcher"/> class.
        /// </summary>
        public HttpRequestDispatcher()
            : base(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestDispatcher"/> class.
        /// </summary>
        /// <param name="handler">
        /// The handler.
        /// </param>
        public HttpRequestDispatcher(HttpMessageHandler handler)
            : base(handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestDispatcher"/> class.
        /// </summary>
        /// <param name="handler">
        /// The handler.
        /// </param>
        /// <param name="disposeHandler">
        /// The dispose handler.
        /// </param>
        public HttpRequestDispatcher(HttpMessageHandler handler, bool disposeHandler)
            : base(handler, disposeHandler)
        {
        }

        string ICommunicationClient.ListenerName { get; set; }

        ResolvedServiceEndpoint ICommunicationClient.Endpoint { get; set; }

        ResolvedServicePartition ICommunicationClient.ResolvedServicePartition { get; set; }
    }
}
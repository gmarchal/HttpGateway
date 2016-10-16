using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Iridium.GatewayCore
{
    /// <summary>
    /// The provider to dispatch HTTP requests to Service Fabric nodes.
    /// </summary>
    public class HttpRequestDispatcherProvider : CommunicationClientFactoryBase<HttpRequestDispatcher>
    {
        private readonly Func<HttpRequestDispatcher> innerDispatcherProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestDispatcherProvider"/> class.
        /// </summary>
        /// <param name="servicePartitionResolver">
        /// The service partition resolver.
        /// </param>
        /// <param name="exceptionHandlers">
        /// The exception handlers.
        /// </param>
        /// <param name="traceId">
        /// The trace id.
        /// </param>
        public HttpRequestDispatcherProvider(
            IServicePartitionResolver servicePartitionResolver = null,
            IEnumerable<IExceptionHandler> exceptionHandlers = null,
            string traceId = null)
            : this(() => new HttpRequestDispatcher(), servicePartitionResolver, exceptionHandlers, traceId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestDispatcherProvider"/> class.
        /// </summary>
        /// <param name="innerDispatcherProvider">
        /// The inner dispatcher provider.
        /// </param>
        /// <param name="servicePartitionResolver">
        /// The service partition resolver.
        /// </param>
        /// <param name="exceptionHandlers">
        /// The exception handlers.
        /// </param>
        /// <param name="traceId">
        /// The trace id.
        /// </param>
        /// <exception cref="ArgumentNullException">The inner dispatcher provider is null.</exception>
        public HttpRequestDispatcherProvider(
            Func<HttpRequestDispatcher> innerDispatcherProvider,
            IServicePartitionResolver servicePartitionResolver = null,
            IEnumerable<IExceptionHandler> exceptionHandlers = null,
            string traceId = null)
            : base(servicePartitionResolver, exceptionHandlers, traceId)
        {
            if (innerDispatcherProvider == null)
            {
                throw new ArgumentNullException(nameof(innerDispatcherProvider));
            }

            this.innerDispatcherProvider = innerDispatcherProvider;
        }

        /// <inheritdoc />
        protected override void AbortClient(HttpRequestDispatcher dispatcher)
        {
            if (dispatcher != null)
            {
                dispatcher.Dispose();
            }
        }

        /// <inheritdoc />
        protected override Task<HttpRequestDispatcher> CreateClientAsync(
            string endpoint,
            CancellationToken cancellationToken)
        {
            var dispatcher = this.innerDispatcherProvider.Invoke();
            dispatcher.BaseAddress = new Uri(endpoint, UriKind.Absolute);

            return Task.FromResult(dispatcher);
        }

        /// <inheritdoc />
        protected override bool ValidateClient(HttpRequestDispatcher dispatcher)
        {
            return dispatcher != null && dispatcher.BaseAddress != null;
        }

        /// <inheritdoc />
        protected override bool ValidateClient(string endpoint, HttpRequestDispatcher dispatcher)
        {
            return dispatcher != null && dispatcher.BaseAddress == new Uri(endpoint, UriKind.Absolute);
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;

namespace Iridium.GatewayCore
{
    /// <summary>
    /// Defines the options for the gateway.
    /// </summary>
    public class GatewayOptions
    {
        /// <summary>
        /// Gets or sets the service uri.
        /// </summary>
        public Uri ServiceUri { get; set; }

        /// <summary>
        /// Gets or sets the target replica selector.
        /// </summary>
        public TargetReplicaSelector TargetReplicaSelector { get; set; }

        /// <summary>
        /// Gets or sets the listener name.
        /// </summary>
        public string ListenerName { get; set; }

        /// <summary>
        /// Gets or sets the relative path.
        /// </summary>
        public Uri RelativePath { get; set; }

        /// <summary>
        /// Gets or sets the operation retry settings.
        /// </summary>
        public OperationRetrySettings OperationRetrySettings { get; set; }

        /// <summary>
        /// Gets or sets the selector for the service partition key.
        /// </summary>
        public Func<HttpContext, ServicePartitionKey> ServicePartitionKeySelector { get; set; }

        public int? Port { get; set; }
    }
}
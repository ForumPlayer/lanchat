using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lanchat.Core.Api;
using Lanchat.Core.Node;
using Lanchat.Core.NodesDetection;

namespace Lanchat.Core
{
    /// <summary>
    ///     Main class representing network in P2P mode.
    /// </summary>
    public interface IP2P
    {
        /// <see cref="P2P.NodesDetection" />
        NodesDetector NodesDetection { get; }

        /// <summary>
        ///     List of connected nodes.
        /// </summary>
        List<INode> Nodes { get; }

        /// <summary>
        ///     Send data to all nodes.
        /// </summary>
        Broadcast Broadcast { get; }

        /// <summary>
        ///     New node connected. After receiving this handlers for node events can be created.
        /// </summary>
        event EventHandler<INode> NodeCreated;

        /// <summary>
        ///     Start server.
        /// </summary>
        void Start();

        /// <summary>
        ///     Connect to node.
        /// </summary>
        /// <param name="ipAddress">Node IP address.</param>
        /// <param name="port">Node port.</param>
        Task<bool> Connect(IPAddress ipAddress, int? port = null);
    }
}
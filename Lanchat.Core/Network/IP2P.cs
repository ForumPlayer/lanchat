using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lanchat.Core.Api;
using Lanchat.Core.Encryption;
using Lanchat.Core.NodesDetection;

namespace Lanchat.Core.Network
{
    /// <summary>
    ///     Main class representing network in P2P mode.
    /// </summary>
    public interface IP2P
    {
        /// <see cref="Lanchat.Core.NodesDetection" />
        NodesDetector NodesDetection { get; }

        /// <summary>
        ///     List of connected nodes.
        /// </summary>
        List<INode> Nodes { get; }

        /// <see cref="Lanchat.Core.Api.IBroadcast" />
        IBroadcast Broadcast { get; }

        /// <see cref="ILocalRsa" />
        ILocalRsa LocalRsa { get; }

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

        /// <summary>
        ///     Connect to node using hostname.
        /// </summary>
        /// <param name="hostname">Node hostname.</param>
        /// <param name="port">Node port.</param>
        Task<bool> Connect(string hostname, int? port = null);
    }
}
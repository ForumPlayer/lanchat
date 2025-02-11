using System;
using System.Net.Sockets;
using Lanchat.Core.Api;
using Lanchat.Core.Chat;
using Lanchat.Core.Encryption;
using Lanchat.Core.FileTransfer;
using Lanchat.Core.Identity;
using Lanchat.Core.TransportLayer;

namespace Lanchat.Core.Network
{
    /// <summary>
    ///     Connected user.
    /// </summary>
    public interface INode
    {
        /// <summary>
        ///     ID of TCP client or session.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        ///     Node ready. If set to false node won't send or receive messages.
        /// </summary>
        bool Ready { get; }

        /// <inheritdoc cref="IUser" />
        IUser User { get; }

        /// <inheritdoc cref="IHost" />
        IHost Host { get; }

        /// <inheritdoc cref="IMessaging" />
        IMessaging Messaging { get; }

        /// <inheritdoc cref="FileTransfer.FileReceiver" />
        IFileReceiver FileReceiver { get; }

        /// <inheritdoc cref="FileTransfer.FileSender" />
        IFileSender FileSender { get; }

        /// <inheritdoc cref="IOutput" />
        IOutput Output { get; }

        /// <inheritdoc cref="INodeRsa" />
        INodeRsa NodeRsa { get; }

        /// <summary>
        ///     Node successful connected and ready to data exchange.
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        ///     Node disconnected. Cannot reconnect.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        ///     TCP session or client returned error.
        /// </summary>
        event EventHandler<SocketError> SocketErrored;

        /// <summary>
        ///     Disconnect from node.
        /// </summary>
        void Disconnect();

        /// <inheritdoc cref="Filesystem.INodeInfo.Trusted"/>
        bool Trusted {get;set;}
    }
}
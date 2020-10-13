﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using Lanchat.Core.Models;

namespace Lanchat.Core.Network
{
    /// <summary>
    ///     Sending and receiving data using this class.
    /// </summary>
    public class NetworkOutput
    {
        private readonly Node node;
        private readonly JsonSerializerOptions serializerOptions;

        internal NetworkOutput(Node node)
        {
            this.node = node;
            serializerOptions = CoreConfig.JsonSerializerOptions;
        }

        /// <summary>
        ///     Send message.
        /// </summary>
        /// <param name="content">Message content.</param>
        public void SendMessage(string content)
        {
            if (!node.Ready)
            {
                return;
            }

            SendData(DataTypes.Message, node.Encryption.Encrypt(content));
        }

        /// <summary>
        ///     Send ping.
        /// </summary>
        public void SendPing()
        {
            if (!node.Ready)
            {
                return;
            }

            SendData(DataTypes.Ping);
        }

        internal void SendHandshake()
        {
            var handshake = new Handshake
            {
                Nickname = CoreConfig.Nickname,
                PublicKey = node.Encryption.ExportPublicKey()
            };

            SendData(DataTypes.Handshake, handshake);
        }

        internal void SendKey()
        {
            var keyInfo = node.Encryption.ExportAesKey();
            SendData(DataTypes.KeyInfo, keyInfo);
        }

        internal void SendNodesList(IEnumerable<IPAddress> list)
        {
            var stringList = list.Select(x => x.ToString());
            SendData(DataTypes.NodesList, stringList);
        }

        private void SendData(DataTypes dataType, object content = null)
        {
            var data = new Wrapper {Type = dataType, Data = content};
            node.NetworkElement.SendAsync(JsonSerializer.Serialize(data, serializerOptions));
        }
    }
}
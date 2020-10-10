﻿using System.Net;
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
            serializerOptions = Config.JsonSerializerOptions;
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

            var message = new Message {Content = content};
            var data = new Wrapper {Type = DataTypes.Message, Data = message};
            node.NetworkElement.SendAsync(JsonSerializer.Serialize(data, serializerOptions));
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

            var data = new Wrapper {Type = DataTypes.Ping};
            node.NetworkElement.SendAsync(JsonSerializer.Serialize(data, serializerOptions));
        }

        internal void SendHandshake()
        {
            var handshake = new Handshake
            {
                Nickname = Config.Nickname, 
                PublicKey = node.Encryption.ExportPublicKey()
            };
            
            var data = new Wrapper {Type = DataTypes.Handshake, Data = handshake};
            node.NetworkElement.SendAsync(JsonSerializer.Serialize(data, serializerOptions));
        }

        internal void SendKey()
        {
            var keyInfo = node.Encryption.ExportAesKey();
            var data = new Wrapper {Type = DataTypes.KeyInfo, Data = keyInfo};
            node.NetworkElement.SendAsync(JsonSerializer.Serialize(data, serializerOptions));
        }

        internal void SendNewNodeInfo(IPAddress ipAddress)
        {
            if (!node.Ready)
            {
                return;
            }
            
            var ip = ipAddress.ToString();
            var data = new Wrapper {Type = DataTypes.NewNodeInfo, Data = ip};
            node.NetworkElement.SendAsync(JsonSerializer.Serialize(data, serializerOptions));
        }
    }
}
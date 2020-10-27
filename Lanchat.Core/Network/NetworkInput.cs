﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Lanchat.Core.Models;

namespace Lanchat.Core.Network
{
    public class NetworkInput
    {
        private readonly Node node;
        private readonly JsonSerializerOptions serializerOptions;

        internal NetworkInput(Node node)
        {
            this.node = node;
            serializerOptions = CoreConfig.JsonSerializerOptions;
        }

        /// <summary>
        ///     Message received.
        /// </summary>
        public event EventHandler<string> MessageReceived;

        /// <summary>
        ///     Ping received.
        /// </summary>
        public event EventHandler PingReceived;

        internal event EventHandler<Handshake> HandshakeReceived;
        internal event EventHandler<KeyInfo> KeyInfoReceived;
        internal event EventHandler<List<IPAddress>> NodesListReceived;
        internal event EventHandler<string> NicknameChanged;

        internal void ProcessReceivedData(object sender, string json)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Wrapper>(json, serializerOptions);
                var content = data.Data?.ToString();

                // If node isn't ready ignore every messages except handshake and key info
                if (!node.Ready && data.Type != DataTypes.Handshake && data.Type != DataTypes.KeyInfo)
                {
                    return;
                }

                switch (data.Type)
                {
                    case DataTypes.Message:
                        MessageReceived?.Invoke(this, TruncateAndValidate(node.Encryption.Decrypt(content), CoreConfig.MaxMessageLenght));
                        break;

                    case DataTypes.Ping:
                        PingReceived?.Invoke(this, EventArgs.Empty);
                        break;

                    case DataTypes.Handshake:
                        var handshake = JsonSerializer.Deserialize<Handshake>(content);
                        handshake.Nickname = TruncateAndValidate(handshake.Nickname, CoreConfig.MaxNicknameLenght);
                        HandshakeReceived?.Invoke(this, handshake);
                        break;

                    case DataTypes.KeyInfo:
                        var keyInfo = JsonSerializer.Deserialize<KeyInfo>(content);
                        KeyInfoReceived?.Invoke(this, keyInfo);
                        break;

                    case DataTypes.NodesList:
                        var stringList = JsonSerializer.Deserialize<List<string>>(content);
                        var list = new List<IPAddress>();

                        // Convert strings to ip addresses
                        stringList.ForEach(x =>
                        {
                            if (IPAddress.TryParse(x, out var ipAddress))
                            {
                                list.Add(ipAddress);
                            }
                        });

                        NodesListReceived?.Invoke(this, list);
                        break;
                    
                    case DataTypes.NicknameUpdate:
                        NicknameChanged?.Invoke(this, TruncateAndValidate(content, CoreConfig.MaxNicknameLenght));
                        break;

                    case DataTypes.Goodbye:
                        node.NetworkElement.EnableReconnecting = false;
                        break;
                    
                    default:
                        Trace.WriteLine("Unknown type received");
                        break;
                }
            }

            // Input errors catching
            catch (Exception ex)
            {
                if (ex is JsonException || ex is ArgumentNullException)
                {
                    Trace.WriteLine("Invalid data received");
                }
                else
                {
                    throw;
                }
            }
        }
        
        private static string TruncateAndValidate(string value, int maxLength)
        {
            value = value.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException();
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "..."; 
        }
    }
}
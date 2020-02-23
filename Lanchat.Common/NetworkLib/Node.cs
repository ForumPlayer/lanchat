﻿using Lanchat.Common.Cryptography;
using Lanchat.Common.NetworkLib.Events;
using Lanchat.Common.NetworkLib.Handlers;
using Lanchat.Common.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace Lanchat.Common.NetworkLib
{
    /// <summary>
    /// Represents network node.
    /// </summary>
    public class Node : IDisposable
    {
        /// <summary>
        /// Node constructor with known port.
        /// </summary>
        /// <param name="ip">Node IP</param>
        internal Node(IPAddress ip)
        {
            Events = new NodeEvents();
            Ip = ip;
            SelfAes = new Aes();
            NicknameNum = 0;
            State = Status.Waiting;
            HandshakeTimer = new Timer { Interval = 5000, Enabled = true };
            HeartbeatTimer = new Timer { Interval = 1200, Enabled = false };
            WaitForHandshake();
        }

        /// <summary>
        /// Nickname without number.
        /// </summary>
        public string ClearNickname { get; private set; }

        /// <summary>
        /// Handshake.
        /// </summary>
        public Handshake Handshake { get; set; }

        /// <summary>
        /// Heartbeat counter.
        /// </summary>
        public int HearbeatCount { get; set; } = 0;

        /// <summary>
        /// Last heartbeat status.
        /// </summary>
        public bool Heartbeat { get; set; }

        /// <summary>
        /// Node IP.
        /// </summary>
        public IPAddress Ip { get; set; }

        /// <summary>
        /// Node mute value.
        /// </summary>
        public bool Mute { get; set; }

        /// <summary>
        /// Node nickname. If nicknames are duplicated returns nickname with number.
        /// </summary>
        public string Nickname
        {
            get
            {
                if (NicknameNum != 0)
                {
                    return ClearNickname + $"#{NicknameNum}";
                }
                else
                {
                    return ClearNickname;
                }
            }
            set => ClearNickname = value;
        }

        /// <summary>
        /// Node TCP port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Node <see cref="Status"/>.
        /// </summary>
        public Status State { get; set; }

        internal Client Client { get; set; }
        internal NodeEvents Events { get; set; }
        internal NodeEventsHandlers EventsHandlers { get; set; }
        internal Timer HandshakeTimer { get; set; }
        internal Timer HeartbeatTimer { get; set; }
        internal int NicknameNum { get; set; }
        internal Aes RemoteAes { get; set; }
        internal Aes SelfAes { get; set; }
        internal Socket Socket { get; set; }

        internal void AcceptHandshake(Handshake handshake)
        {
            Handshake = handshake;
            Nickname = handshake.Nickname;

            if (Port == 0)
            {
                Port = handshake.Port;
                CreateConnection();
                Events.OnHandshakeAccepted();
            }

            Client.SendKey(new Key(
                     Rsa.Encode(SelfAes.Key, Handshake.PublicKey),
                     Rsa.Encode(SelfAes.IV, Handshake.PublicKey)));
        }

        internal void CreateConnection()
        {
            Client = new Client(this);
            Client.Connect(Ip, Port);
        }

        internal void CreateRemoteAes(string key, string iv)
        {
            RemoteAes = new Aes(key, iv);

            State = Status.Ready;
            Events.OnStateChange();

            StartHeartbeat();
        }

        internal void Process()
        {
            byte[] response;

            while (true)
            {
                response = new byte[Socket.ReceiveBufferSize];
                _ = Socket.Receive(response);

                if (!Socket.IsConnected())
                {
                    Trace.WriteLine($"[HOST] Socket closed ({Ip})");
                    Socket.Close();
                    Events.OnNodeDisconnected(Ip);
                    break;
                }

                try
                {
                    var respBytesList = new List<byte>(response);
                    var data = Encoding.UTF8.GetString(respBytesList.ToArray());

                    // Parse jsons
                    IList<JObject> buffer = new List<JObject>();

                    JsonTextReader reader = new JsonTextReader(new StringReader(data))
                    {
                        SupportMultipleContent = true
                    };

                    using (reader)
                    {
                        while (true)
                        {
                            if (!reader.Read())
                            {
                                break;
                            }

                            JsonSerializer serializer = new JsonSerializer();
                            JObject packet = serializer.Deserialize<JObject>(reader);

                            buffer.Add(packet);
                        }
                    }

                    // Process all parsed jsons from buffer
                    foreach (JObject packet in buffer)
                    {
                        IList<JToken> obj = packet;
                        var type = ((JProperty)obj[0]).Name;
                        var content = ((JProperty)obj[0]).Value;

                        // Events

                        if (type == "handshake")
                        {
                            Events.OnReceivedHandshake(content.ToObject<Handshake>());
                        }

                        if (type == "key")
                        {
                            Events.OnReceivedKey(content.ToObject<Key>());
                        }

                        if (type == "heartbeat")
                        {
                            Events.OnReceivedHeartbeat();
                        }

                        // Data

                        if (type == "message")
                        {
                            Events.OnReceivedMessage(content.ToString());
                        }

                        if (type == "nickname")
                        {
                            Events.OnChangedNickname(content.ToString());
                        }

                        if (type == "list")
                        {
                            Events.OnReceivedList(content.ToObject<List<ListItem>>(), IPAddress.Parse(((IPEndPoint)Socket.LocalEndPoint).Address.ToString()));
                        }
                    }
                }
                catch (DecoderFallbackException)
                {
                    Trace.WriteLine($"[HOST] Data processing error: utf8 decode gone wrong ({Ip})");
                }
                catch (JsonReaderException)
                {
                    Trace.WriteLine($"([HOST] Data processing error: not vaild json ({Ip})");
                }
            }
        }

        internal void StartHeartbeat()
        {
            HeartbeatTimer.Elapsed += new ElapsedEventHandler(OnHeartebatOver);
            HeartbeatTimer.Start();

            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (disposedValue)
                    {
                        break;
                    }
                    else
                    {
                        Client.SendHeartbeat();
                    }
                }
            }).Start();
        }

        internal void StartProcess()
        {
            new System.Threading.Thread(() =>
            {
                try
                {
                    Process();
                }
                catch (SocketException)
                {
                    // Disconnect node on exception
                    Trace.WriteLine($"[HOST] Socket exception. Node will be disconnected ({Ip})");
                    Events.OnNodeDisconnected(Ip);
                    Socket.Close();
                }
            }).Start();
        }

        internal void WaitForHandshake()
        {
            // Wait for handshake
            HandshakeTimer.Start();
        }

        // Hearbeat over event
        private void OnHeartebatOver(object o, ElapsedEventArgs e)
        {
            // If heartbeat was not received make count negative
            if (Heartbeat)
            {
                // Reset heartbeat
                Heartbeat = false;

                // Count heartbeat
                if (HearbeatCount < 0)
                {
                    HearbeatCount = 1;
                }
                else
                {
                    HearbeatCount++;
                }

                // Change state
                if (State == Status.Suspended)
                {
                    State = Status.Resumed;
                    Events.OnStateChange();
                }
            }
            else
            {
                // Count heartbeat
                if (HearbeatCount > 0)
                {
                    HearbeatCount = -1;
                }
                else
                {
                    HearbeatCount--;
                }

                // Change state
                if (State != Status.Suspended)
                {
                    State = Status.Suspended;
                    Events.OnStateChange();
                }
            }
        }

        // Dispose

        #region IDisposable Support

        private bool disposedValue = false;

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Node()
        {
            Dispose(false);
        }

        /// <summary>
        /// Node dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Node dispose.
        /// </summary>
        /// <param name="disposing"> Free any other managed objects</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (HeartbeatTimer != null)
                    {
                        HeartbeatTimer.Dispose();
                    }
                    if (Client != null)
                    {
                        Client.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
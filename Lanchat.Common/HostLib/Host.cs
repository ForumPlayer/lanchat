﻿using Lanchat.Common.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lanchat.Common.HostLib
{
    internal static class SocketExtensions
    {
        internal static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }

    internal class Host
    {
        private readonly int port;

        private readonly UdpClient udpClient;

        internal Host(int port)
        {
            Events = new HostEvents();
            this.port = port;
            udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        internal HostEvents Events { get; set; }

        internal void ListenBroadcast()
        {
            Task.Run(() =>
            {
                var from = new IPEndPoint(0, 0);
                while (true)
                {
                    var recvBuffer = udpClient.Receive(ref from);

                    // Try parse
                    try
                    {
                        var paperplane = JsonConvert.DeserializeObject<Paperplane>(Encoding.UTF8.GetString(recvBuffer));
                        Events.OnReceivedBroadcast(paperplane, from.Address);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Paperplane parsing error: " + e.Message);
                    }
                }
            });
        }

        internal void StartBroadcast(object self)
        {
            Task.Run(() =>
              {
                  while (true)
                  {
                      var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(self));
                      udpClient.Send(data, data.Length, "255.255.255.255", port);
                      Thread.Sleep(1000);
                  }
              });
        }
        internal void StartHost(int port)
        {
            Task.Run(() =>
            {
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = -1,
                };

                server.Bind(new IPEndPoint(IPAddress.Any, port));
                server.Listen(-1);

                while (true)
                {
                    var socket = server.Accept();
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    new Thread(() =>
                    {
                        try
                        {
                            Process(socket);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Socket connection processing error: " + ex.Message);
                        }
                    }).Start();
                }
            });

            // Host client process
            void Process(Socket socket)
            {
                byte[] response;
                int received;
                var ip = IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());

                Events.OnNodeConnected(ip);

                while (true)
                {
                    response = new byte[socket.ReceiveBufferSize];
                    received = socket.Receive(response);

                    if (!socket.IsConnected())
                    {
                        socket.Close();
                        Events.OnNodeDisconnected(ip);
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

                        // Process all parsed jsons from buffer
                        foreach (JObject packet in buffer)
                        {
                            IList<JToken> obj = packet;
                            var type = ((JProperty)obj[0]).Name;
                            var content = ((JProperty)obj[0]).Value;

                            // Normal events

                            if (type == "handshake")
                            {
                                Events.OnReceivedHandshake(content.ToObject<Handshake>(), ip);
                            }

                            if (type == "key")
                            {
                                Events.OnReceivedKey(content.ToObject<Key>(), ip);
                            }

                            if (type == "heartbeat")
                            {
                                Events.OnReceivedHeartbeat(ip);
                            }

                            if (type == "message")
                            {
                                Events.OnReceivedMessage(content.ToString(), ip);
                            }

                            if (type == "nickname")
                            {
                                Events.OnChangedNickname(content.ToString(), ip);
                            }


                            if (type == "list")
                            {
                                Events.OnReceivedList(content.ToObject<List<ListItem>>());
                            }

                            // Requests

                            if (type == "request")
                            {
                                // Request type: nickname
                                if (content.ToString() == "nickname")
                                {
                                    Events.OnReceivedRequest("nickname", ip);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // Handle decoder exception
                        if (e is DecoderFallbackException)
                        {
                            Trace.WriteLine("Data processing error: utf8 decode gone wrong");
                            Trace.WriteLine($"Sender: {ip}");
                        }

                        // Handle json parse exception
                        else if (e is JsonReaderException)
                        {
                            Trace.WriteLine("Data processing error: not vaild json");
                            Trace.WriteLine($"Sender: {ip}");
                        }

                        // Handle other exceptions
                        else
                        {
                            Trace.WriteLine($"Data processing error: {e.GetType()}");
                            Trace.WriteLine($"Sender: {ip}");
                        }
                    }
                }

                Trace.WriteLine($"Socket for {ip} closed");
            }
        }
    }
}
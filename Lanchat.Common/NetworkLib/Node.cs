﻿using Lanchat.Common.CryptographyLib;
using Lanchat.Common.HostLib;
using Lanchat.Common.HostLib.Types;
using System;
using System.Diagnostics;
using System.Net;
using System.Timers;

namespace Lanchat.Common.NetworkLib
{
    public class Node
    {
        public Node(Guid id, int port, IPAddress ip)
        {
            Id = id;
            Port = port;
            Ip = ip;
            SelfAes = new AesInstance();
            NicknameNum = 0;
        }

        public void CreateConnection()
        {
            Client = new Client(this);
            Client.Connect(Ip, Port);
        }

        public void AcceptHandshake(Handshake handshake)
        {
            Nickname = handshake.Nickname;
            PublicKey = handshake.PublicKey;
            Client.SendKey(new Key(
                Rsa.Encode(SelfAes.Key, PublicKey),
                Rsa.Encode(SelfAes.IV, PublicKey)));
        }

        public void CreateRemoteAes(string key, string iv)
        {
            RemoteAes = new AesInstance(key, iv);

            // Start heartbeat
            HeartbeatTimer = new Timer
            {
                Interval = 2000
            };
            HeartbeatTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            HeartbeatTimer.Start();
            Client.Heartbeat();
        }

        // Hearbeat over event
        private void OnTimedEvent(object o, ElapsedEventArgs e)
        {
            if (!Heartbeat)
            {
                Trace.WriteLine("Heartbeat over");
            }
            else
            {
                Client.Heartbeat();
                Heartbeat = false;
            }
        }


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

        public string ClearNickname { get; private set; }
        public int NicknameNum { get; set; }
        public Guid Id { get; set; }
        public string PublicKey { get; set; }
        public bool Mute { get; set; }
        public AesInstance SelfAes { get; set; }
        public AesInstance RemoteAes { get; set; }
        public int Port { get; set; }
        public IPAddress Ip { get; set; }
        public Client Client { get; set; }
        public bool Heartbeat { get; set; }
        public Timer HeartbeatTimer { get; set; }
    }
}
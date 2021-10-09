using System;
using Autofac.Core;
using Lanchat.ClientCore;
using Lanchat.Core.Network;

namespace Lanchat.Avalonia
{
    public static class Lanchat
    {
        public static IP2P Network { get; private set; } = null!;
        public static Config Config { get; private set; } = null!;

        public static void Start(Action<IActivatedEventArgs<INode>> activatedNodeAction)
        {
            Config = Storage.LoadConfig();
            Config.DebugMode = true;
            Config.ConnectToSaved = false;
            Config.StartServer = true;
            Config.NodesDetection = false;
            var rsaDatabase = new RsaDatabase();
            Network = new P2P(Config, rsaDatabase, activatedNodeAction);
            Network.Start();
            Logger.DeleteOldLogs(5);
        }
    }
}
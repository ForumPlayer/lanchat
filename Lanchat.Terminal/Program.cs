﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lanchat.ClientCore;
using Lanchat.Core.Filesystem;
using Lanchat.Core.Network;
using Lanchat.Terminal.Commands;
using Lanchat.Terminal.Handlers;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal
{
    public static class Program
    {
        public static IP2P Network { get; private set; }
        public static Config Config { get; private set; }
        public static INodesDatabase NodesDatabase { get; private set; }
        public static CommandsManager CommandsManager { get; private set; }
        public static Notifications Notifications { get; private set; }

        private static async Task Main(string[] args)
        {
            var tcs = new TaskCompletionSource();
            Console.CancelKeyPress += (_, _) => tcs.SetResult();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => tcs.SetResult();

            var storage = new Storage();
            Config = storage.Config;
            Theme.SetFromThemeModel(storage.Theme);
            NodesDatabase = new NodesDatabase();
            CommandsManager = new CommandsManager();
            Notifications = new Notifications();

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(Config.Language);
            }
            catch
            {
                Trace.WriteLine("Cannot load translation. Using default.");
            }

            Resources.Culture = CultureInfo.CurrentCulture;

            Network = new P2P(
                storage,
                Config,
                NodesDatabase, x =>
            {
                _ = new NodeHandlers(x.Instance);
                _ = new FileTransferHandlers(x.Instance);
            });

            CheckStartArguments(args);
            Window.Initialize();
            Logger.StartLogging();

            try
            {
                Network.Start();
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                TabsManager.HomeView.AddText(Resources.PortBusy, Theme.LogWarning);
            }

            if (args.Contains("--localhost") || args.Contains("-l"))
            {
                await Network.Connect(IPAddress.Loopback);
            }

            Logger.DeleteOldLogs(5);

            await tcs.Task;
            Console.ResetColor();
            Console.Clear();
            Console.CursorVisible = true;
        }

        private static void CheckStartArguments(string[] args)
        {
            if (args.Contains("--no-saved") || args.Contains("-a"))
            {
                Config.ConnectToSaved = false;
            }

            if (args.Contains("--no-udp") || args.Contains("-b"))
            {
                Config.NodesDetection = false;
            }

            if (args.Contains("--no-server") || args.Contains("-n"))
            {
                Config.StartServer = false;
            }

            if (args.Contains("--debug") || args.Contains("-d") || Debugger.IsAttached)
            {
                Config.DebugMode = true;
                Trace.Listeners.Add(new TraceListener());
            }
            else
            {
                _ = UpdateChecker.CheckUpdatesAsync();
            }
        }
    }
}
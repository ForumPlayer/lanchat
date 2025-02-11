﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autofac.Core;
using Lanchat.Core.Api;
using Lanchat.Core.Filesystem;
using Lanchat.Core.Encryption;
using Lanchat.Core.FileSystem;
using Lanchat.Core.NodesDiscovery;
using Lanchat.Core.TransportLayer;

namespace Lanchat.Core.Network
{
    /// <inheritdoc />
    public class P2P : IP2P
    {
        internal readonly IConfig Config;
        private readonly AddressChecker addressChecker;
        private readonly INodesDatabase nodesDatabase;
        private readonly NodesControl nodesControl;
        private readonly Server server;

        /// <summary>
        ///     Initialize P2P mode
        /// </summary>
        /// <param name="storage">IStorage implementation</param>
        /// <param name="config">IConfig implementation</param>
        /// <param name="nodesDatabase">INodesDatabase implementation</param>
        /// <param name="nodeCreated">Method called after creation of new node</param>
        /// <param name="apiHandlers">Optional custom api handlers</param>
        public P2P(
            IStorage storage,
            IConfig config,
            INodesDatabase nodesDatabase,
            Action<IActivatedEventArgs<INode>> nodeCreated,
            IEnumerable<Type> apiHandlers = null)
        {
            Config = config;
            this.nodesDatabase = nodesDatabase;
            LocalRsa = new LocalRsa(nodesDatabase);
            var container = NodeSetup.Setup(storage, config, nodesDatabase, LocalRsa, this, nodeCreated, apiHandlers);
            addressChecker = new AddressChecker(config, nodesDatabase);
            nodesControl = new NodesControl(container, addressChecker, nodesDatabase);
            server = new Server(IPAddress.Any, Config.ServerPort, nodesControl, addressChecker);

            NodesDetection = new NodesDetection(Config);
            Broadcast = new Broadcast(nodesControl.Nodes);
            _ = new ConfigObserver(this);

            NodesDetection.DetectedNodes.CollectionChanged += ConnectToDetectedAddresses;
        }

        /// <inheritdoc />
        public NodesDetection NodesDetection { get; }

        /// <inheritdoc />
        public List<INode> Nodes => nodesControl.Nodes.Where(x => x.Ready).Cast<INode>().ToList();

        /// <inheritdoc />
        public IBroadcast Broadcast { get; }

        /// <inheritdoc />
        public ILocalRsa LocalRsa { get; }

        /// <inheritdoc />
        public void Start()
        {
            if (Config.StartServer)
            {
                server.Start();
            }

            if (Config.NodesDetection)
            {
                NodesDetection.Start();
            }

            if (Config.ConnectToSaved)
            {
                ConnectToSavedAddresses();
            }
        }

        /// <inheritdoc />
        public Task<bool> Connect(IPAddress ipAddress, int? port = null)
        {
            addressChecker.CheckAddress(ipAddress);
            addressChecker.LockAddress(ipAddress);
            var tcs = new TaskCompletionSource<bool>();
            port ??= Config.ServerPort;
            var client = new Client(ipAddress, port.Value);
            var node = nodesControl.CreateNode(client);
            SubscribeEvents(node, tcs);
            client.ConnectAsync();
            return tcs.Task;
        }

        /// <inheritdoc />
        public async Task<bool> Connect(string hostname, int? port = null)
        {
            var dnsResult = await Dns.GetHostAddressesAsync(hostname);
            var ipAddress = dnsResult.FirstOrDefault();
            if (ipAddress == null)
            {
                return false;
            }

            return Connect(ipAddress, port).Result;
        }

        private void ConnectToDetectedAddresses(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null)
            {
                return;
            }

            foreach (DetectedNode newNode in args.NewItems)
            {
                try
                {
                    Connect(newNode.IpAddress);
                }
                catch (ArgumentException)
                { }
            }
        }

        private void ConnectToSavedAddresses()
        {
            nodesDatabase.SavedNodes.ForEach(x =>
            {
                try
                {
                    Connect(x.IpAddress);
                }
                catch (ArgumentException)
                { }
            });
        }

        private static void SubscribeEvents(INodeInternal node, TaskCompletionSource<bool> tcs)
        {
            node.Connected += (_, _) => tcs.TrySetResult(true);
            node.CannotConnect += (_, _) => tcs.TrySetResult(false);
        }
    }
}
﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using Lanchat.Core;
using Lanchat.Core.FileTransfer;
using Lanchat.Core.Models;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal
{
    public class NodeEventsHandlers
    {
        private readonly Node node;

        public NodeEventsHandlers(Node node)
        {
            this.node = node;
            node.Messaging.MessageReceived += OnMessageReceived;
            node.Messaging.PrivateMessageReceived += OnPrivateMessageReceived;

            node.FileReceiver.FileTransferFinished += OnFileTransferFinished;
            node.FileReceiver.FileTransferError += OnFileTransferError;
            node.FileReceiver.FileTransferRequestReceived += OnFileTransferHandlerRequestReceived;

            node.FileSender.FileTransferRequestAccepted += OnFileTransferHandlerRequestAccepted;
            node.FileSender.FileTransferRequestRejected += OnFileTransferHandlerRequestRejected;
            node.FileSender.FileTransferFinished += OnFileTransferFinished;
            node.FileSender.FileTransferError += OnFileTransferError;

            node.Connected += OnConnected;
            node.Disconnected += OnDisconnected;
            node.HardDisconnect += OnHardDisconnected;
            node.SocketErrored += OnSocketErrored;
            node.CannotConnect += OnCannotConnect;
            node.PropertyChanged += OnPropertyChanged;

            Ui.FileTransferProgressMonitor.ObserveNodeTransfers(node.FileReceiver, node.FileSender);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Status":
                    var status = node.Status switch
                    {
                        Status.Online => "online",
                        Status.AwayFromKeyboard => "afk",
                        Status.DoNotDisturb => "dnd",
                        _ => ""
                    };
                    Ui.Log.Add(string.Format(Resources._StatusChange, node.Nickname, status));
                    break;

                case "Nickname":
                    if (!node.Ready) return;
                    Ui.Log.Add(string.Format(Resources._NicknameChanged, node.PreviousNickname, node.Nickname));
                    break;
            }
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._Connected, node.Nickname));
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._Reconnecting, node.Nickname));
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnHardDisconnected(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._Disconnected, node.Nickname));
            Ui.NodesCount.Text = Program.Network.Nodes.Count.ToString();
        }

        private void OnMessageReceived(object sender, string e)
        {
            Ui.Log.AddMessage(e, node.Nickname);
        }

        private void OnPrivateMessageReceived(object sender, string e)
        {
            Ui.Log.AddPrivateMessage(e, node.Nickname);
        }

        private void OnSocketErrored(object sender, SocketError e)
        {
            Ui.Log.Add(string.Format(Resources._ConnectionError, node.Id, e));
        }

        private void OnCannotConnect(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._CannotConnect, node.Id));
        }

        private void OnFileTransferHandlerRequestReceived(object sender, FileTransferRequest e)
        {
            Ui.Log.Add(string.Format(Resources._FileRequest, node.Nickname, e.FileName));
        }

        private void OnFileTransferFinished(object sender, FileTransferRequest e)
        {
            Ui.Log.Add(string.Format(Resources._FileReceived, node.Nickname, Path.GetFullPath(e.FilePath)));
        }

        // TODO: Log node id
        private void OnFileTransferError(object sender, Exception e)
        {
            Ui.Log.Add(string.Format(Resources._FileExchangeError, e.Message));
        }

        private void OnFileTransferHandlerRequestAccepted(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._FileRequestAccepted, node.Nickname));
        }

        private void OnFileTransferHandlerRequestRejected(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._FileRequestRejected, node.Nickname));
        }

        private void OnFileTransferFinished(object sender, EventArgs e)
        {
            Ui.Log.Add(string.Format(Resources._FileTransferFinished, node.Nickname));
        }
    }
}
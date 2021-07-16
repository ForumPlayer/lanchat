using System;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal.Commands.FileTransfer
{
    public class Cancel : ICommand
    {
        public string Alias => "cancel";
        public int ArgsCount => 1;

        public void Execute(string[] args)
        {
            var tabsManager = Program.Window.TabsManager;
            var node = Program.Network.Nodes.Find(x => x.User.ShortId == args[0]);
            if (node == null)
            {
                tabsManager.WriteError(Resources._UserNotFound);
                return;
            }

            try
            {
                node.FileReceiver.CancelReceive(true);
            }
            catch (InvalidOperationException)
            {
                tabsManager.WriteError(Resources._NoFileReceiveRequest);
            }
        }
    }
}
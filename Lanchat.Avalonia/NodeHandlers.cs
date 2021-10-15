using Lanchat.Core.Network;

namespace Lanchat.Avalonia
{
    public class NodeHandlers
    {
        private readonly INode node;
        private readonly MainWindow mainWindow;

        public NodeHandlers(INode node, MainWindow mainWindow)
        {
            this.node = node;
            this.mainWindow = mainWindow;
            node.Messaging.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object? sender, string e)
        {
            mainWindow.AddMessage(node.User.Nickname, e);
        }
    }
}
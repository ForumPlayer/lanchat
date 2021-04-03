using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal.Commands
{
    public class Cancel : ICommand
    {
        public string Alias { get; } = "cancel";
        public int ArgsCount { get; } = 1;

        public void Execute(string[] args)
        {
            var node = Program.P2P.Nodes.Find(x => x.ShortId == args[0]);
            if (node == null)
            {
                Ui.Log.AddError(Resources._UserNotFound);
                return;
            }

            if (!node.FileReceiver.CancelReceive())
            {
                Ui.Log.AddError(Resources._NoFileReceiveRequest);
            }
        }
    }
}
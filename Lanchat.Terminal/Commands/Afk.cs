using Lanchat.Core;
using Lanchat.Core.Models;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal.Commands
{
    public class Afk : ICommand
    {
        public string Alias { get; set; } = "afk";
        public int ArgsCount { get; set; }

        public void Execute(string[] _)
        {
            Program.Config.Status = Status.AwayFromKeyboard;
            Ui.Status.Text = "AFK";
        }
    }
}
namespace Lanchat.Ipc.Commands.General
{
    public class Nick : ICommand
    {
        public string Alias => "nick";
        public int ArgsCount => 1;
        public int ContextArgsCount => ArgsCount;

        public void Execute(string[] args)
        {
            var nickname = args[0].Trim();
            if (nickname.Length >= 20 || string.IsNullOrWhiteSpace(nickname))
            {
                Program.IpcSocket.Send("invalid_argument");
            }
            else
            {
                Program.Config.Nickname = nickname;
            }
        }
    }
}
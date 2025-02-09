namespace Lanchat.Ipc.Commands
{
    public interface ICommand
    {
        string Alias { get; }
        int ArgsCount { get; }
        void Execute(string[] args);
    }
}
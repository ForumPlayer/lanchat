using System;
using System.IO;
using System.Linq;
using System.Security;
using Lanchat.Core.Network;
using Lanchat.Terminal.Properties;
using Lanchat.Terminal.UserInterface;

namespace Lanchat.Terminal.Commands.FileTransfer
{
    public class Send : ICommand
    {
        public string[] Aliases { get; } =
        {
            "send"
        };
        public int ArgsCount => 2;
        public int ContextArgsCount => 1;

        public void Execute(string[] args)
        {
            var node = Program.Network.Nodes.Find(x => x.User.ShortId == args[0]);
            if (node == null)
            {
                Writer.WriteError(Resources.UserNotFound);
                return;
            }

            Execute(args.Skip(1).ToArray(), node);
        }

        public void Execute(string[] args, INode node)
        {
            var filePath = string.Join(" ", args);
            try
            {
                node.FileSender.CreateSendRequest(filePath);
                Writer.WriteStatus(Resources.FileRequestSent);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case FileNotFoundException:
                    case UnauthorizedAccessException:
                    case SecurityException:
                    case PathTooLongException:
                    case ArgumentException:
                        Writer.WriteError(string.Format(Resources.CannotAccessFile, filePath));
                        break;

                    case InvalidOperationException:
                        Writer.WriteError(Resources.FileTransferInProgress);
                        break;
                }
            }
        }
    }
}
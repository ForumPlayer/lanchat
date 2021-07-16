using System;
using System.IO;
using System.Security;
using Lanchat.Terminal.Properties;

namespace Lanchat.Terminal.Commands.FileTransfer
{
    public class Send : ICommand
    {
        public string Alias => "send";
        public int ArgsCount => 2;
        
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
                node.FileSender.CreateSendRequest(args[1]);
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
                        tabsManager.WriteError(string.Format(Resources._CannotAccessFile, args[1]));
                        break;

                    case InvalidOperationException:
                        tabsManager.WriteError(Resources._FileTransferInProgress);
                        break;
                }
            }
        }
    }
}
using System.ComponentModel;
using Lanchat.Core.Identity.Models;
using Lanchat.Core.Network;

namespace Lanchat.Core.Filesystem
{
    internal class ConfigObserver
    {
        private readonly P2P network;

        public ConfigObserver(P2P network)
        {
            this.network = network;
            network.Config.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Nickname":
                    network.Broadcast.SendData(new NicknameUpdate { NewNickname = network.Config.Nickname });
                    break;

                case "UserStatus":
                    network.Broadcast.SendData(new UserStatusUpdate { NewUserStatus = network.Config.UserStatus });
                    break;
            }
        }
    }
}
using Lanchat.Core.Api;
using Lanchat.Core.Network.Models;
using Lanchat.Core.Tcp;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace Lanchat.Core.Network.Handlers
{
    internal class ConnectionControlHandler : ApiHandler<ConnectionControl>
    {
        private readonly IHost host;

        internal ConnectionControlHandler(IHost host)
        {
            this.host = host;
        }

        protected override void Handle(ConnectionControl connectionControl)
        {
            switch (connectionControl.Status)
            {
                case ConnectionStatus.RemoteDisconnect:
                    host.Close();
                    break;
            }
        }
    }
}
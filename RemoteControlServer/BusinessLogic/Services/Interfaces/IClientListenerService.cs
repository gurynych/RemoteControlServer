using RemoteControlServer.Data.Interfaces;
using System.Net.Sockets;

namespace RemoteControlServer.BusinessLogic.Services.Interfaces
{
    public interface IClientListenerService
    {
        TcpListener GetTcpListener(INetworkMessage networkMessage);

        Task SendAsync();

        Task StartAsync();

        void Stop();
    }
}

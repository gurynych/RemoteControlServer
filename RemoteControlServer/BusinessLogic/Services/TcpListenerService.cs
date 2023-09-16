using System.Net;
using System.Net.Sockets;

namespace RemoteControlServer.BusinessLogic.Services
{
    public class TcpListenerService //: IClientListenerService
    {
        private TcpListener listener;

        public TcpListenerService()
        {
            listener = new TcpListener(IPAddress.Any, 11000);
            listener.Start();
        }

        public TcpListener GetTcpListener()
        {
            return listener;
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkFileSync
{
    class ConnectionListener : NetworkConnector
    {
        public ConnectionListener(IPAddress address, int port) : base (address, port)
        {
        }

        public TcpClient Listen()
        {
            var listener = new TcpListener(_address, _port);
            listener.Start();
            return listener.AcceptTcpClient();
        }
    }
}
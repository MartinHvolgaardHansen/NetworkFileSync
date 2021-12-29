using System.Net;
using System.Net.Sockets;

namespace NetworkFileSync
{
    class ClientConnector : NetworkConnector
    {
        public ClientConnector(IPAddress address, int port) : base(address, port)
        {
        }

        public TcpClient Connect() 
        {
            var client = new TcpClient();
            client.Connect(_address, _port);
            return client;
        }
    }
}
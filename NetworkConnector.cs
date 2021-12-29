using System.Net;

namespace NetworkFileSync
{
    abstract class NetworkConnector
    {
        protected IPAddress _address;
        protected int _port;

        protected NetworkConnector(IPAddress address, int port)
        {
            _address = address;
            _port = port;
        }
    }
}
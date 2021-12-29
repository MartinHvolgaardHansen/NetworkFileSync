using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkFileSync
{
    class TcpTransceiver<T>
    {
        private IEncoder<T> _encoder;
        private int packetSize;

        public TcpTransceiver(IEncoder<T> encoder, int packetSize = 1024)
        {
            _encoder = encoder;
            this.packetSize = packetSize;
        }

        public async Task BeginReceive(TcpClient client, Action<T> onPacketReceived)
        {
            await Task.Run(() => 
            {
                var stream = client.GetStream();
                while (client.Connected) 
                {
                    byte[] data = new byte[packetSize]; 
                    stream.Read(data, 0, data.Length); 
                    onPacketReceived(_encoder.Decode(data));
                }
                client.Close();
            });
        }

        public async Task BeginSend(TcpClient client, T entity)
        {
            await Task.Run(() =>
            {
                byte[] data = _encoder.Encode(entity);
                var stream = client.GetStream();
                stream.Write(data, 0, data.Length); 
            });
        }
    }
}
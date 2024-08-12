using App.Contacts;
using App.Contracts.Extensions;
using System.Net;
using System.Net.Sockets;

namespace Infrastructure.Provider
{


    public class MessageSource : IMessageSource
    {
        private readonly UdpClient _udpClient;
        public MessageSource( int localPort)
        {
            _udpClient = new UdpClient(localPort);
        }

        public MessageSource(UdpClient udpClient)
        {
            _udpClient = udpClient;
        }
               

        public async Task<ReceiveResult> ReceiveMessage(CancellationToken cancellationToken)
        {
            var data = await _udpClient.ReceiveAsync(cancellationToken);
            return new (data.RemoteEndPoint, data.Buffer.BytesToMessage());
        }

        public async Task SendMessage(Message message, IPEndPoint endPoint, CancellationToken cancellationToken)
        {
            await _udpClient.SendAsync(message.MessageToBytes(), endPoint, cancellationToken);
        }
    }
}

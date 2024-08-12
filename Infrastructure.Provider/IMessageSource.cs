using App.Contacts;
using System.Net;


namespace Infrastructure.Provider
{
    public interface IMessageSource
    {
        Task<ReceiveResult> ReceiveMessage(CancellationToken cancellationToken);
        Task SendMessage(Message message, IPEndPoint endPoint, CancellationToken cancellationToken);
        //IPEndPoint CreateEndPoint(string address, int port);
        //IPEndPoint GetServerEndPoint();
    }
}

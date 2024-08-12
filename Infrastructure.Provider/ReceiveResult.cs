using App.Contacts;
using System.Net;

namespace Infrastructure.Provider
{
    public record ReceiveResult(IPEndPoint EndPoint, Message message);
}

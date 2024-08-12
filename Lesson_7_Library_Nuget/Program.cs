using Core;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Provider;
using System.Net;
using System.Net.Sockets;

namespace Lesson_7_Library_Nuget
{
    internal class Program
    {       
        
        static async Task Main(string[] args)
        {
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12000);
            UdpClient udpClient;
            IMessageSource source;

            if (args.Length == 0)
            {
                //server
                udpClient = new UdpClient(serverEP);
                source = new MessageSource(udpClient);
                var chat = new ChatServer (source, new ChatContext());
                await chat.Start();
            }
            else
            {
                //client               
                source = new MessageSource(int.Parse(args[1]));
                var chat = new ChatClient (args[0],serverEP,source);
                await chat.Start();

            }           
        }
    }
}

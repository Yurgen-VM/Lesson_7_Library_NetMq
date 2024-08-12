using App.Contacts;
using Infrastructure.Provider;
using System.Net;

namespace Core
{
    public class ChatClient : ChatBase
    {
        private readonly User _user;
        private readonly IPEndPoint _serverEP;
        private readonly IMessageSource _source;
        private IEnumerable<User> _users = [];

        public ChatClient(string userName, IPEndPoint serverEP, IMessageSource source)
        {
            _user = new User { Name = userName };
            _serverEP = serverEP;
            _source = source;
        }

        public override async Task Start()
        {
            var join = new Message { Text = _user.Name, Command = Command.Join };
            await _source.SendMessage(join, _serverEP, CancellationToken);

            Task.Run(Listener);

            while (!CancellationToken.IsCancellationRequested)
            {
                string input = (await Console.In.ReadLineAsync()) ?? String.Empty;
                Message message;
                if (input.Trim().ToLower() == "exit")
                {
                    message = new() {SenderId = _user.Id, Command = Command.Exit };
                }
                else
                {
                    message = new() { Text = input, SenderId = _user.Id, Command = Command.None };
                }
                await _source.SendMessage(message, _serverEP, CancellationToken);


            }
        }

        protected override async Task Listener()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    ReceiveResult result = await _source.ReceiveMessage(CancellationToken);
                    if (result.message is null)
                    {
                        throw new Exception("Message is null");
                    }

                    if (result.message.Command == Command.Join)
                    {
                        JoinHandler(result.message);
                    }
                    else if (result.message.Command == Command.Users)
                    {
                        UsersHandler(result.message);
                    }
                    else if (result.message.Command == Command.None)
                    {
                        MessageHandler(result.message);
                    }
                }
                catch (Exception exc)
                {
                    await Console.Out.WriteLineAsync(exc.Message);
                }
            }
        }

        private User GetUser()
        {
            return _user;
        }

        //public async Task ClientSender()
        //{
        //    while (true)
        //    {
        //        Console.WriteLine("Введите сообщение");
        //        string? message = Console.ReadLine();
        //        Console.WriteLine("Укажите имя получателя");
        //        string? recipient = Console.ReadLine();
        //        if (string.IsNullOrEmpty(recipient))
        //        {
        //            continue;
        //        }
        //        var msgJson = new Message()
        //        {
        //            Text = message!,
        //            RecepientId = _name,
        //            ToName = recipient,
        //            Command = Command.Message

        //        };
        //        _messageSource.SendMessage(msgJson, serverEP);
        //    }
        //}


        private void MessageHandler(Message message)
        {
            Console.WriteLine($"{_users.First(u => u.Id == message.SenderId)}: {message.Text}"); ;
        }

        private void UsersHandler(Message message)
        {
            _users = message.Users;
        }

        private void JoinHandler(Message message)
        {
            _user.Id = message.RecepientId;
            Console.WriteLine("Join success");
        }
    }
}

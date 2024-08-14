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
            // Task.Run(ClientSender);
            Task.Run(Listener);


            //await Task.Run(Listener);

            while (!CancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Введите сообщение");
                string inputText = (await Console.In.ReadLineAsync()) ?? String.Empty;
                Console.WriteLine("Укажите получателя");
                string inputRecipient = (await Console.In.ReadLineAsync()) ?? String.Empty;
                Message message;
                if (inputText.Trim().ToLower() == "exit")
                {
                    message = new() { SenderId = _user.Id, Command = Command.Exit };
                }
                else
                {
                    message = new() { Text = inputText, RecipientName = inputRecipient, SenderName = _user.Name, Command = Command.None };
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


        private void MessageHandler(Message message)
        {
            Console.WriteLine($"Сообщение от {message.SenderName}: {message.Text}");
        }

        private void UsersHandler(Message message)
        {
            _users = message.Users;
            Console.Write("Пользователи on-line: ");
            foreach (User user in _users)
            {
                Console.Write(user.Name + " ");
            }
            Console.WriteLine();
        }

        private void JoinHandler(Message message)
        {
            _user.Id = message.RecepientId;
            Console.WriteLine($"Сообщение от {message.SenderName}: {message.Text}");
        }
    }
}

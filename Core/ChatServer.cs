using App.Contacts;
using Domain;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Provider;
using Microsoft.EntityFrameworkCore;

namespace Core
{
    public class ChatServer : ChatBase
    {
        private readonly IMessageSource _source;
        private readonly ChatContext _context;
        private HashSet<User> _users = [];

        public ChatServer(IMessageSource source, ChatContext context)
        {
            _source = source;
            _context = context;
        }

        public override async Task Start()
        {
            await Task.CompletedTask;
            await Task.Run(Listener);
        }

        protected override async Task Listener()
        {
            Console.WriteLine("Сервер ожидает сообщения");
            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {                   
                    ReceiveResult result = await _source.ReceiveMessage(CancellationToken);
                    if (result.message is null)
                    {
                        throw new Exception("Message is null");
                    }

                    switch (result.message.Command)
                    {
                        case Command.None:
                            await MessageHandller(result);
                            break;
                        case Command.Join:
                            await JoinHandller(result);
                            break;
                        case Command.Exit:
                            await ExitHandller(result);
                            break;
                        case Command.Users:
                            await UsersHandller(result);
                            break;
                        case Command.Confirm:
                            break;
                    }
                }
                catch (Exception exc)
                {
                    await Console.Out.WriteLineAsync(exc.Message);
                }
            }
        }

        private async Task UsersHandller(ReceiveResult result)
        {
            throw new NotImplementedException();
        }

        private async Task ExitHandller(ReceiveResult result)
        {
            var user = User.FromDomain(await _context.Users.FirstAsync(u => u.Id == result.message.SenderId));
            user.LastOnLine = DateTime.Now;
            await _context.SaveChangesAsync();
            _users.Remove(_users.First(u => u.Id == result.message.SenderId));
        }

        private async Task MessageHandller(ReceiveResult result)
        {
            if (result.message.RecepientId < 0)
            {
                await SendAllAsync(result.message);
            }
            else
            {
                await _source.SendMessage
                (
                result.message,
                _users.First(u => u.Id == result.message.SenderId).endPoint!,
                CancellationToken
                );

                var recipientEndPoint = _users.FirstOrDefault(u => u.Id == result.message.SenderId)?.endPoint;

                if (recipientEndPoint is not null)
                {
                    await _source.SendMessage
                    (
                    result.message,
                    recipientEndPoint,
                    CancellationToken
                    );
                }
            }
        }

        // Метод для подтверждения получения сообщения
        public static void ConfirmMessageReceived(int? id)
        {
            Console.WriteLine("Сообщение принято id = " + id);

            // Изменяем статус получения сообщения в базе данных
            using (ChatContext ctx = new ChatContext())
            {
                var msg = ctx.Messages.FirstOrDefault(x => x.Id == id);
                if (msg != null)
                {
                    msg.DeliveryStatus = true;
                    ctx.SaveChanges();
                }
            }
        }


        private async Task JoinHandller(ReceiveResult result)
        {
            //Добавляем пользователя в базу данных (если его еще там нет)
            using (ChatContext ctx = new ChatContext())
            {
                if (ctx.Users.FirstOrDefault(x => x.Name == result.message.Text) != null) return;
                ctx.Add(new UserEntity { Name = result.message.Text, LastOnLine = DateTime.UtcNow });
                ctx.SaveChanges();
            }

            User? user = _users.FirstOrDefault(u => u.Name == result.message.Text);
            if (user is null)
            {
                user = new User { Name = result.message.Text };
                _users.Add(user);
            }
            user.endPoint = result.EndPoint;

            await _source.SendMessage
                (
                new Message { Command = Command.Join, RecepientId = user.Id },
                user.endPoint,
                CancellationToken
                );

            await SendAllAsync(new Message { Command = Command.Confirm, Text = $"{user.Name} присоединился к чату" });
            await SendAllAsync(new Message { Command = Command.Users, RecepientId = user.Id, Users = _users });
            
            var unreaded = await _context.Messages.Where(u => u.RecepientId == user.Id).ToListAsync();
            foreach( var message in unreaded )
            {
                await _source.SendMessage
                (
                Message.FromDomain(message),
                user.endPoint,
                CancellationToken
                );
            }
        }

        private async Task SendAllAsync(Message message)
        {
            foreach (var user in _users)
            {
                await _source.SendMessage
               (
               message,
               user.endPoint,
               CancellationToken
               );
            }
        }
    }
}

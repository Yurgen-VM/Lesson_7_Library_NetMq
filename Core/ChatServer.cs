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
            Console.WriteLine($"Получено сообщение {result.message.Text} от {result.message.SenderName} для {result.message.RecipientName}");

            //  Проверяем есть ли получатель в базе данных

            var userEntity = await _context.Users.FirstOrDefaultAsync(n => n.Name == result.message.RecipientName);

            if (userEntity == null) // Если получаетеля не существует, пересылаем сообщение всем пользователям
            {
                await SendAllAsync(result.message);
            }
            else
            {
                var recipientEndPoint = _users.FirstOrDefault(u => u.Name == result.message.RecipientName)?.endPoint;

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

        private async Task JoinHandller(ReceiveResult result)
        {
            //Добавляем пользователя в базу данных (если его еще там нет)
            using (ChatContext ctx = new ChatContext())
            {
                if (ctx.Users.FirstOrDefault(x => x.Name == result.message.Text) == null)
                {
                    ctx.Add(new UserEntity { Name = result.message.Text, LastOnLine = DateTime.UtcNow });
                    ctx.SaveChanges();
                }
            }

            User? user = _users.FirstOrDefault(u => u.Name == result.message.Text);
            if (user is null)
            {
                user = new User { Name = result.message.Text };
                _users.Add(user);
            }
            user.endPoint = result.EndPoint;


            await SendAllAsync(new Message { Command = Command.Join, SenderName = "Server", Text = $"{user.Name} присоединился к чату" });

            // Отпрвляем список пользователей on-line

            await SendAllAsync(new Message { Command = Command.Users, RecipientName = user.Name, Users = _users });

            var unreaded = await _context.Messages.Where(u => u.RecepientId == user.Id).ToListAsync();
            foreach (var message in unreaded)
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
               user.endPoint!,
               CancellationToken
               );
            }
        }
    }
}

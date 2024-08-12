using Domain;
using System.Data;

namespace App.Contacts
{

    public class Message
    { 
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecepientId { get; set; } = -1;
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
        public string Text { get; set; } = String.Empty;
        public Command Command { get; set; } = Command.None;
        public IEnumerable<User> Users  { get; set; } = [];
        public bool DeliveryStatus { get; set; } = false;
        public static Message FromDomain(MessageEntity msgEntity)
        {
            return new Message
            {
                Id = msgEntity.Id,
                SenderId = msgEntity.SenderId,
                RecepientId = msgEntity.RecepientId,
                CreatedAt = msgEntity.CreatedAt,
                DeliveryStatus = msgEntity.DeliveryStatus,
            };
        }
    }
}



using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class MessageEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Text { get; set; }
        public int SenderId { get; set; }
        public int RecepientId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public bool DeliveryStatus { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

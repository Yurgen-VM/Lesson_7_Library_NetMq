using App.Contacts;
using System.Text;
using System.Text.Json;

namespace App.Contracts.Extensions
{
    public static class MesaageExtensions
    {
        public static Message? BytesToMessage( this byte[] data)
              => JsonSerializer.Deserialize<Message>(Encoding.UTF8.GetString(data));

        public static byte[] MessageToBytes (this Message message)
              => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    }
}

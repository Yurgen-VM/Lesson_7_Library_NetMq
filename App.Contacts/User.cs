using Domain;
using System.Net;
using System.Text.Json.Serialization;

namespace App.Contacts
{
    public record User
    {
       
        public int Id {  get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime LastOnLine { get; set; } = DateTime.Now;
        
        [JsonIgnore]
        public IPEndPoint? endPoint { get; set; }

        public static User FromDomain(UserEntity userEntity) => new User()
        {
            Id = userEntity.Id,
            Name = userEntity.Name,
            LastOnLine = userEntity.LastOnLine,
        };

    }
}

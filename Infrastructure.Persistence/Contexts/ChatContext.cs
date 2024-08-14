
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Contexts
{
    public class ChatContext: DbContext
    {
        public DbSet<UserEntity> Users {  get; set; }
        public DbSet<MessageEntity> Messages { get; set; }            

        
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            // LogLevel.None - параметр отвечающий за тип выводмой информации в лог.
            builder.LogTo(Console.WriteLine, LogLevel.None).UseLazyLoadingProxies().UseNpgsql("Host = localhost; Username = postgres; Password = example; Database = Chat_DB_Work;");

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ConfigurateUsers(builder);
            ConfigurateMessages(builder);
        }

        private static void ConfigurateMessages(ModelBuilder builder)
        {
            builder.Entity<MessageEntity>().HasKey(x => x.Id);
            builder.Entity<MessageEntity>().Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Entity<MessageEntity>().HasOne<UserEntity>().WithMany().HasForeignKey(x => x.SenderId);
            builder.Entity<MessageEntity>().HasOne<UserEntity>().WithMany().HasForeignKey(x => x.RecepientId);
            builder.Entity<MessageEntity>().Property(x => x.DeliveryStatus).HasColumnName("Delivery_Status");
            builder.Entity<MessageEntity>().Property(x => x.SenderName).HasColumnName("Sender_Name");
            builder.Entity<MessageEntity>().Property(x => x.RecipientName).HasColumnName("Recipient_Name");
        }

        private static void ConfigurateUsers(ModelBuilder builder)
        {
            builder.Entity<UserEntity>().HasKey(x => x.Id);
            builder.Entity<UserEntity>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<UserEntity>().HasIndex(x => x.Name).IsUnique();
            builder.Entity<UserEntity>().Property(x => x.LastOnLine);
        }
    }
}

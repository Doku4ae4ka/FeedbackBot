//using Microsoft.EntityFrameworkCore;
//using FeedbackBot.Application.Interfaces;
//using FeedbackBot.Domain.Models.Entities;
//using FeedbackBot.Persistence.EntityTypeConfiguration;

//namespace FeedbackBot.Persistence.DbContexts;

//public class ChatsDbContext : DbContext
//{
//    public DbSet<Chat> Chats { get; set; }

//    public ChatsDbContext(DbContextOptions<ChatsDbContext> options)
//        : base(options) { }

//    protected override void OnModelCreating(ModelBuilder builder)
//    {
//        builder.ApplyConfiguration(new ChatConfiguration());
//        base.OnModelCreating(builder);
//    }
//}

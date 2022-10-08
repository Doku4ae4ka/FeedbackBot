//using Microsoft.EntityFrameworkCore;
//using FeedbackBot.Application.Interfaces;
//using FeedbackBot.Domain.Models.Entities;
//using FeedbackBot.Persistence.EntityTypeConfiguration;

//namespace FeedbackBot.Persistence.DbContexts;

//public class UsersDbContext : DbContext
//{
//    public DbSet<User> Users { get; set; }

//    public UsersDbContext(DbContextOptions<UsersDbContext> options)
//        : base(options) { }

//    protected override void OnModelCreating(ModelBuilder builder)
//    {
//        builder.ApplyConfiguration(new UserConfiguration());
//        base.OnModelCreating(builder);
//    }
//}

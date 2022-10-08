//using FeedbackBot.Domain.Models.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace FeedbackBot.Persistence.EntityTypeConfiguration;

//public class UserConfiguration : IEntityTypeConfiguration<User>
//{
//    public void Configure(EntityTypeBuilder<User> builder)
//    {
//        builder.HasKey(user => user.Id);
//        builder.HasIndex(user => user.Id).IsUnique();
//        builder.Property(user => user.FirstName).HasMaxLength(50);

//    }
//}

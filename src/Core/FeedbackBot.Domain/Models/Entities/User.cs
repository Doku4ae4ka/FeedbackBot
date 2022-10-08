namespace FeedbackBot.Domain.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public string FirstName { get; set; } = null!;
}

namespace FeedbackBot.Application.Models.DTOs;

public class EmailResponseDto
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public string Body { get; set; } = null!;
}

namespace FeedbackBot.Application.Models.DTOs;

public class MessageDto
{
    public long Id { get; set; }
    public string Text { get; set; } = null!;
    public UserDto Sender { get; set; } = null!;
    public ChatDto Chat { get; set; } = null!;
    public MessageDto? ReplyTarget { get; set; } = null!;
    public bool IsReplyToMe { get; set; }
    public bool IsPrivate { get; set; }
}
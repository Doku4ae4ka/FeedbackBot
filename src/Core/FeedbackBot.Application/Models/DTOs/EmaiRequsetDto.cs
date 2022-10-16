namespace FeedbackBot.Application.Models.DTOs;

public class EmailRequest
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Partners { get; set; } = null!;
    public string Grade { get; set; } = null!;
    public string Profile { get; set; } = null!;
    public string RequestSubject { get; set; } = null!;
    public string RequestContent { get; set; } = null!;
    public string EmailAdress { get; set; } = null!;
    public DateTime Created { get; set; }
}


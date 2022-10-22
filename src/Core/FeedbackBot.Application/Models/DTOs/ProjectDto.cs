namespace FeedbackBot.Application.Models.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Members { get; set; } = null!;
    public string Grade { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string SchoolEmail { get; set; } = null!;
    public DateTime Created { get; set; }
}


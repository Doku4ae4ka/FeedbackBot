using System.ComponentModel.DataAnnotations;

namespace FeedbackBot.Domain.Models.Entities;

public class User
{
    public long Id { get; set; }
    [MaxLength(50)] public string FirstName { get; set; } = null!;
}

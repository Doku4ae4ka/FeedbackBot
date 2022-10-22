using FeedbackBot.Domain.Models.Entities;

namespace FeedbackBot.Application.Interfaces;

public interface IEmailService
{
    public Task SendEmailAsync(Project context);
    public Task ReceiveEmailAsync(Project context);

}
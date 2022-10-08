using FeedbackBot.Application.Models.Contexts;
using FeedbackBot.Application.Models.Resources;

namespace FeedbackBot.Application.Interfaces;

public interface ICommand
{
    public Task ExecuteAsync(CommandContext context, CancellationToken cancellationToken);
}

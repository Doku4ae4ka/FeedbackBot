using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;

namespace FeedbackBot.Application.Commands;

public class StartCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var response = context.Resources!.Get("Greeting", context.Message.Sender.FirstName);
        await context.ReplyAsync(response);
    }
}

using Telegram.Bot.Types;

namespace FeedbackBot.Application.Interfaces;

public interface IUpdateHandler
{
    public Task HandleAsync(Update update, CancellationToken cancellationToken);
}
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.DTOs;
using FeedbackBot.Application.Models.Resources;
using Telegram.Bot;

namespace FeedbackBot.Application.Models.Contexts;

public class CallbackQueryContext : ContextBase<ResourcesBase>
{
    public CallbackQueryDto Query = null!;

    private readonly ITelegramBotClient _bot;

    public CallbackQueryContext(
        ITelegramBotClient bot,
        ICheckpointMemoryService checkpoints,
        IReplyMemoryService replies)
        : base(bot, checkpoints, replies) => _bot = bot;

    public async Task ShowAlertAsync(string text) =>
        await _bot.AnswerCallbackQueryAsync(Query.Id, text, true);
    
    public async Task ShowNotificationAsync(string text) =>
        await _bot.AnswerCallbackQueryAsync(Query.Id, text);
}
using Microsoft.VisualBasic;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using FeedbackBot.Application.Models.Resources;

namespace FeedbackBot.Application.Models.Contexts;

public class BehaviorContext : MessageContext
{
    public string BehaviorTypeName = null!;
    public BehaviorResources? Resources;

    public BehaviorContext(
        ITelegramBotClient bot)
        : base(bot) { }

    public override async Task<int> ReplyAsync(
        string text,
        bool? disableWebPagePreview = null) =>
        await base.ReplyAsync(text, disableWebPagePreview);
}

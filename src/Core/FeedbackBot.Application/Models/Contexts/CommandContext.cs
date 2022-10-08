using FeedbackBot.Application.Models.Resources;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace FeedbackBot.Application.Models.Contexts;

public class CommandContext : MessageContext
{
    public string CommandTypeName = null!;
    public CommandResources? Resources;
    public CommandContext(ITelegramBotClient bot)
    : base(bot) { }

    public override async Task<int> ReplyAsync(
    string text,
    bool? disableWebPagePreview = null) =>
    await base.ReplyAsync(text, disableWebPagePreview);

}

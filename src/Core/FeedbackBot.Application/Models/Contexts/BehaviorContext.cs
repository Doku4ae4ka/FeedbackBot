using Microsoft.VisualBasic;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using FeedbackBot.Application.Models.Resources;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Checkpoints;

namespace FeedbackBot.Application.Models.Contexts;

public class BehaviorContext : MessageContext
{
    public string BehaviorTypeName = null!;
    public BehaviorResources? Resources;

    public BehaviorContext(
        ITelegramBotClient bot,
        IInteractionService interaction)
        : base(bot, interaction) { }

    public Checkpoint SetMentionCheckpoint()
    {
        Checkpoint = Interaction.IssueMentionCheckpoint(Message.Sender.Id);
        return Checkpoint;
    }

    public override async Task<int> ReplyAsync(
        string text,
        bool? disableWebPagePreview = null,
        string? handlerTypeName = null) =>
        await base.ReplyAsync(text, disableWebPagePreview,
            handlerTypeName ?? BehaviorTypeName);
}

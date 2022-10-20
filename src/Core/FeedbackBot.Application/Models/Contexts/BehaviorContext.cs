using Telegram.Bot;
using FeedbackBot.Application.Models.Resources;
using FeedbackBot.Application.Interfaces;

namespace FeedbackBot.Application.Models.Contexts;

public class BehaviorContext : ContextBase<BehaviorResources>
{
    public BehaviorContext(
        ITelegramBotClient bot,
        ICheckpointMemoryService checkpoint,
        IReplyMemoryService replies)
        : base(bot, checkpoint, replies) { }
}
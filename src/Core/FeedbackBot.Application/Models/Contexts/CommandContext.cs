using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Checkpoints;
using FeedbackBot.Application.Models.DTOs;
using FeedbackBot.Application.Models.Resources;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace FeedbackBot.Application.Models.Contexts;

public class CommandContext : ContextBase<CommandResources>
{
    public MessageDto Message = null!;
    public string Payload = null!;
    
    public CommandContext(
        ITelegramBotClient bot,
        ICheckpointMemoryService checkpoints,
        IReplyMemoryService replies)
        : base(bot, checkpoints, replies) { }
}
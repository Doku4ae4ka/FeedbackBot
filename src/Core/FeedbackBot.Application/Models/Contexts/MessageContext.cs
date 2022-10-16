using FeedbackBot.Application.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Microsoft.VisualBasic;
using FeedbackBot.Application.Models.Checkpoints;
using FeedbackBot.Application.Interfaces;

namespace FeedbackBot.Application.Models.Contexts;

public class MessageContext
{
    public MessageDto Message = null!;
    public Checkpoint? Checkpoint;
    public ITelegramBotClient Bot;

    protected readonly IInteractionService Interaction;

    public MessageContext(ITelegramBotClient bot, IInteractionService interaction)
    {
        Bot = bot;
        Interaction = interaction;
    }

    public void ResetCheckpoint()
    {
        Interaction.ResetCheckpoint(Message.Sender.Id);
        Checkpoint = null;
    }

    public virtual async Task<int> ReplyAsync(
        string text,
        bool? disableWebPagePreview = null,
        string? handlerTypeName = null)
    {
        var message = await Bot.SendTextMessageAsync(Message.Chat.Id, text,
            disableWebPagePreview: disableWebPagePreview ?? true);

        if (handlerTypeName is not null)
            await Interaction.SetPreviousReplyMessageIdAsync(
                handlerTypeName, message.Chat.Id, message.MessageId);

        return message.MessageId;
    }

    public async Task DeleteMessageAsync(int messageId) =>
        await Bot.DeleteMessageAsync(Message.Chat.Id, messageId);
}
using FeedbackBot.Application.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Microsoft.VisualBasic;

namespace FeedbackBot.Application.Models.Contexts;

public class MessageContext
{
    public MessageDto Message = null!;
    public ITelegramBotClient Bot;

    //protected readonly IInteractionService Interaction;

    public MessageContext(ITelegramBotClient bot)
    {
        Bot = bot;
    }

    //public void ResetCheckpoint()
    //{
    //    Interaction.ResetCheckpoint(Message.Sender.Id);
    //    Checkpoint = null;
    //}
    public virtual async Task<int> ReplyAsync(
        string text,
        bool? disableWebPagePreview = null)
    {
        var message = await Bot.SendTextMessageAsync(Message.Chat.ChatId, text,
            disableWebPagePreview: disableWebPagePreview ?? true);

        return message.MessageId;
    }

    //public async Task DeleteMessageAsync(int messageId) =>
    //    await Bot.DeleteMessageAsync(Message.Chat.Id, messageId);
}
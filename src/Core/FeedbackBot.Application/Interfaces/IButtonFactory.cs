using Telegram.Bot.Types.ReplyMarkups;

namespace FeedbackBot.Application.Interfaces;

public interface IButtonFactory
{
    public InlineKeyboardButton CreateCallbackDataButton(string label, string data);
}
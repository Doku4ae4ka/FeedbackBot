using FeedbackBot.Application.Delegates;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;
using Telegram.Bot.Types.Enums;

namespace FeedbackBot.Application.Behaviors;

public class MentionBehavior : IBehavior
{
    private readonly ILinguisticParser _parser;

    public MentionBehavior(ILinguisticParser parser) =>
        _parser = parser;

    public async Task HandleAsync(BehaviorContext context, BehaviorContextHandler next)
    {
        if (context.Update.Type != UpdateType.Message)
        {
            await next(context);
            return;
        }
        
        var mention = _parser.TryParseMentionFromBeginning(context.Update.Message!.Text);
        if (mention is null || !string.IsNullOrWhiteSpace(
                context.Update.Message.Text.Substring(mention.Segment.Length)))
        {
            await next(context);
            return;
        }

        context.SetCheckpoint("BotMentioned");
        await context.SendTextAsync(context.Resources!.GetRandom<string>("AtYourService"));
    }
}
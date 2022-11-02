using FeedbackBot.Application.Delegates;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;

namespace FeedbackBot.Application.Behaviors;

public class MisunderstandingBehavior : IBehavior
{
    public async Task HandleAsync(BehaviorContext context, BehaviorContextHandler next)
    {
        if (context.Update.Message!.IsReplyToMe)
        {
            await next(context);
            return;
        }
        
        var checkpoint =  context.GetCheckpoint();
        var isBotMentioned = checkpoint is { Name: "BotMentioned" };
        
        if (context.Update.Message.IsPrivate ||
            context.Update.Message.IsReplyToMe ||
            isBotMentioned)
        {
            if (isBotMentioned)
                context.ResetCheckpoint();
            
            await context.SendTextAsync(context.Resources!.GetRandom<string>("CannotUnderstandYou"));
            return;
        }

        await next(context);
    }
}
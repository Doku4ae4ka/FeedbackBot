using FeedbackBot.Application.Delegates;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;

namespace FeedbackBot.Application.Behaviors;

public class MisunderstandingBehavior : IBehavior
{
    public async Task HandleAsync(BehaviorContext context, BehaviorContextHandler next)
    {
        if (context.Message.IsReplyToMe)
        {
            await next(context);
            return;
        }

        if (context.Message.IsPrivate ||
            context.Message.IsReplyToMe)
        {
            await context.ReplyAsync(context.Resources!.Get("CannotUnderstandYou"));
            return;
        }

        await next(context);
    }
}
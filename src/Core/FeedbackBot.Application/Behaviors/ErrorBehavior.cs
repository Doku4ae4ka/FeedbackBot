using FeedbackBot.Application.Delegates;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;
using Serilog;

namespace FeedbackBot.Application.Behaviors;

public class ErrorBehavior : IBehavior
{
    public async Task HandleAsync(BehaviorContext context, BehaviorContextHandler next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            Log.Error(e, "Behavior pipeline has thrown an exception");
            await context.SendTextAsync(context.Resources!.GetRandom<string>("SomethingWentWrong"));
        }
    }
}
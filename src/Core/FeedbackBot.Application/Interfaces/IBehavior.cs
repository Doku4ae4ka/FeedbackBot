using FeedbackBot.Application.Delegates;
using FeedbackBot.Application.Models.Contexts;

namespace FeedbackBot.Application.Interfaces;

public interface IBehavior
{
    public Task HandleAsync(BehaviorContext context, BehaviorContextHandler next);
}
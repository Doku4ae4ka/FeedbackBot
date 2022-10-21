using FeedbackBot.Application.Delegates;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Checkpoints;
using FeedbackBot.Application.Models.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types.Enums;

namespace FeedbackBot.Application.Behaviors;

public abstract class CommandBehaviorBase : IBehavior
{
    private readonly ICommandsService _commands;
    private readonly IResourcesService _resources;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CommandContext _commandContext;
    private readonly ICheckpointMemoryService _checkpoints;

    public CommandBehaviorBase(
        ICommandsService commands,
        IResourcesService resources,
        IServiceScopeFactory scopeFactory,
        CommandContext commandContext,
        ICheckpointMemoryService checkpoints)
    {
        _commands = commands;
        _resources = resources;
        _scopeFactory = scopeFactory;
        _commandContext = commandContext;
        _checkpoints = checkpoints;
    }

    public async Task HandleAsync(BehaviorContext context, BehaviorContextHandler next)
    {
        if (context.Update.Type != UpdateType.Message)
        {
            await next(context);
            return;
        }

        var message = context.Update.Message!;

        // Fill command context
        _commandContext.Update = context.Update;
        _commandContext.Message = message;
        // _commandContext.Payload = message.NormalizedText;
        
        // Extract command from checkpoint if possible
        var checkpoint = context.GetCheckpoint();
        if (checkpoint is not null && checkpoint.HandlerTypeName.EndsWith("Command"))
        {
            _commandContext.HandlerTypeName = checkpoint.HandlerTypeName;
            _commandContext.Resources = _resources.GetCommandResources(_commandContext.HandlerTypeName);
        }
        else
        {
            var considerCommand = FillCommandContext(_commandContext, context);
            if (!considerCommand || _commandContext.Resources is null)
            {
                await next(context);
                return;
            }
        }

        using var scope = _scopeFactory.CreateScope();
        var command = _commands.GetCommandInstance(scope, _commandContext.HandlerTypeName);
        await command.ExecuteAsync(_commandContext, new CancellationTokenSource().Token);
    }

    protected abstract bool FillCommandContext(CommandContext commandContext, BehaviorContext behaviorContext);
}
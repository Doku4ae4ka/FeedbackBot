using FeedbackBot.Application.Delegates;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Checkpoints;
using FeedbackBot.Application.Models.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace FeedbackBot.Application.Behaviors.Base;

public abstract class CommandBehaviorBase : IBehavior
{
    private readonly ICommandsService _commands;
    private readonly IResourcesService _resources;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CommandContext _commandContext;

    public CommandBehaviorBase(
        ICommandsService commands,
        IResourcesService resources,
        IServiceScopeFactory scopeFactory,
        CommandContext commandContext)
    {
        _commands = commands;
        _resources = resources;
        _scopeFactory = scopeFactory;
        _commandContext = commandContext;
    }

    public async Task HandleAsync(BehaviorContext context, BehaviorContextHandler next)
    {
        _commandContext.Message = context.Message;
        _commandContext.Checkpoint = context.Checkpoint;
        
        if (_commandContext.Checkpoint is CommandCheckpoint commandCheckpoint)
        {
            _commandContext.CommandTypeName = commandCheckpoint.CommandTypeName;
            _commandContext.Resources = _resources.GetCommandResources(_commandContext.CommandTypeName);
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
        var command = _commands.GetCommandInstance(scope, _commandContext.CommandTypeName);
        await command.ExecuteAsync(_commandContext, new CancellationTokenSource().Token);
    }

    protected abstract bool FillCommandContext(CommandContext commandContext, BehaviorContext behaviorContext);
}
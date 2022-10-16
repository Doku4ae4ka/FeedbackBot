using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;
using FeedbackBot.Application.Models.DTOs;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FeedbackBot.Application.Common.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly IResourcesService _resources;
    private readonly ICommandsService _commands;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CommandContext _commandContext;
    private readonly BehaviorContext _behaviorContext;

    public UpdateHandler(
        ICommandsService commands,
        IResourcesService resources,
        IServiceScopeFactory scopeFactory,
        CommandContext commandContext,
        BehaviorContext behaviorContext)
    {
        _commands = commands;
        _resources = resources;
        _scopeFactory = scopeFactory;
        _commandContext = commandContext;
        _behaviorContext = behaviorContext;
}


    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        // Handle text messages only
        if (update.Message?.Text is null)
            return;

#if DEBUG
        Log.Debug("Received message: {@0}", update.Message);
#else
        Log.Information("Received message ({0}) from chat {1}: {2}",
            update.Message.MessageId, update.Message.Chat.Id, update.Message.Text);
#endif

        FillBehaviorContext(_behaviorContext, update.Message);
        FillCommandContext(_commandContext, _behaviorContext);

        if (_commandContext.CommandTypeName != null)
        {
            using var scope = _scopeFactory.CreateScope();
            var command = _commands.GetCommandInstance(scope, _commandContext.CommandTypeName);
            await command.ExecuteAsync(_commandContext, new CancellationTokenSource().Token);
        }
        else
        {
            var response = "Я вас не понял";
            await _behaviorContext.ReplyAsync(response);
        }
    }
    private void FillBehaviorContext(BehaviorContext context, Message message)
    {
        context.Message = message.Adapt<MessageDto>();
        context.Message.IsReplyToMe = context.Message.ReplyTarget?.Sender.Id == context.Bot.BotId;
        context.Message.IsPrivate = message.Chat.Type == ChatType.Private;
    }

    private bool FillCommandContext(CommandContext commandContext, BehaviorContext behaviorContext)
    {
        if (string.IsNullOrWhiteSpace(behaviorContext.Message.Text))
            return false;

        var slash = behaviorContext.Message.Text.Split().First().ToLower();
        if (!slash.StartsWith('/'))
        {
            Log.Information("First word is not a slash");
            return true;
        }

        slash = slash[1..].ToString().ToLower();

        var commandTypeName = _commands.GetCommandTypeNameBySlash(slash);
        if (commandTypeName is null)
        {
            Log.Information("No command with the given slash found in message");
            return false;
        }

        commandContext.CommandTypeName = commandTypeName;
        commandContext.Resources = _resources.GetCommandResources(commandContext.CommandTypeName);
        commandContext.Message = behaviorContext.Message;
        return true;
    }
}
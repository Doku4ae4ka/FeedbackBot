using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;

namespace FeedbackBot.Application.Behaviors;

public class SlashCommandBehavior : CommandBehaviorBase
{
    private readonly ITelegramBotClient _bot;
    private readonly ICommandsService _commands;
    private readonly IResourcesService _resources;

    public SlashCommandBehavior(
        ITelegramBotClient bot,
        ICommandsService commands,
        IResourcesService resources,
        IServiceScopeFactory scopeFactory,
        CommandContext commandContext,
        ICheckpointMemoryService checkpoints)
        : base(commands, resources, scopeFactory, commandContext, checkpoints)
    {
        _bot = bot;
        _commands = commands;
        _resources = resources;
    }

    protected override bool FillCommandContext(CommandContext commandContext, BehaviorContext behaviorContext)
    {
        if (string.IsNullOrWhiteSpace(commandContext.Message.Text))
            return false;

        var slash = commandContext.Message.Text.Split().First().ToLower();
        if (!slash.StartsWith('/'))
        {
            Log.Information("First word is not a slash");
            return true;
        }

        slash = slash[1..].ToLower();
        
        if (slash.Contains('@'))
        {
            var bot = _bot.GetMeAsync().Result;
            var postfix = $"@{bot.Username!.ToLower()}";
            
            if (slash.EndsWith(postfix))
                slash = slash[..^postfix.Length].ToString();
            else
                return true;
        }

        var commandTypeName = _commands.GetCommandTypeNameBySlash(slash);
        if (commandTypeName is null)
        {
            Log.Information("No command with the given slash found in message");
            return false;
        }

        commandContext.HandlerTypeName = commandTypeName;
        commandContext.Resources = _resources.GetCommandResources(commandContext.HandlerTypeName);
        commandContext.Payload = commandContext.Message.Text[slash.Length..].TrimStart();
        return true;
    }
}

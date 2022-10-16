using FeedbackBot.Application.Behaviors.Base;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;

namespace FeedbackBot.Application.Behaviors;

public class SlashCommandBehavior : CommandBehaviorBase
{
    private readonly ICommandsService _commands;
    private readonly IResourcesService _resources;

    public SlashCommandBehavior(
        ICommandsService commands,
        IResourcesService resources,
        IServiceScopeFactory scopeFactory,
        CommandContext commandContext)
        : base(commands, resources, scopeFactory, commandContext)
    {
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

        slash = slash[1..].ToString().ToLower();
        
        if (slash.Contains('@'))
        {
            var bot = commandContext.Bot.GetMeAsync().Result;
            var postfix = $"@{bot.Username.ToLower()}";
            
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

        commandContext.CommandTypeName = commandTypeName;
        commandContext.Resources = _resources.GetCommandResources(commandContext.CommandTypeName);
        //commandContext.Payload = commandContext.Message.Text[slash.Length..].TrimStart();
        return true;
    }
}
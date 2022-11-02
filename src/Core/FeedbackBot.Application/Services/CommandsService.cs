using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace FeedbackBot.Application.Services;

public class CommandsService : ICommandsService
{
    private readonly IDictionary<string, CommandResources> _commandResources;
    
    public CommandsService(IDictionary<string, CommandResources> commandResources) =>
        _commandResources = commandResources;

    public ICommand GetCommandInstance(IServiceScope scope, string commandTypeName)
    {
        var commandType = Type.GetType(commandTypeName)!;
        return (ICommand)scope.ServiceProvider.GetRequiredService(commandType);
    }
    
    public string? GetCommandTypeNameByAlias(string alias)
    {
        var commandAliasMap = _commandResources.Select(x => new
        {
            CommandTypeName = x.Key,
            Aliases = x.Value.Aliases
        }).ToArray();
            
        var commandAlias = commandAliasMap.FirstOrDefault(x => x.Aliases.Contains(alias));
        return commandAlias?.CommandTypeName;
    }
    
    public string? GetCommandTypeNameBySlash(string slash)
    {
        var commandSlashMap = _commandResources.Select(x => new
        {
            CommandTypeName = x.Key,
            Slashes = x.Value.Slashes
        }).ToArray();
            
        var commandSlash = commandSlashMap.FirstOrDefault(x => x.Slashes.Contains(slash));
        return commandSlash?.CommandTypeName;
    }
}
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;
using FeedbackBot.Application.Models.Resources;

namespace FeedbackBot.Application.Commands;

public class AliasesCommand : ICommand
{
    private readonly IEnumerable<CommandResources> _commands;
    private readonly ILinguisticParser _parser;

    public AliasesCommand(IEnumerable<CommandResources> commands, ILinguisticParser parser)
    {
        _commands = commands;
        _parser = parser;
    }

    public async Task ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var alias = _parser.TryParseCommandAliasFromBeginning(context.Payload);
        if (alias is null)
        {
            await context.SendTextAsync(context.Resources!.GetRandom<string>("CommandNotFound"));
            return;
        }

        var aliases = _commands
            .First(x => x.Aliases.Contains(alias.Case))
            .Aliases.ToArray();
        
        await context.SendTextAsync(aliases.Any()
            ? string.Join('\n', aliases)
            : context.Resources!.GetRandom<string>("SingleAlias"));
    }
}
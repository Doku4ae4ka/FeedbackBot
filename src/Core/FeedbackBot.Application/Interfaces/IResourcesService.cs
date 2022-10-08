using FeedbackBot.Application.Models.Resources;

namespace FeedbackBot.Application.Interfaces;

public interface IResourcesService
{
    public CommandResources? GetCommandResources<TCommand>() where TCommand : ICommand;
    public CommandResources? GetCommandResources(string commandTypeName);
}

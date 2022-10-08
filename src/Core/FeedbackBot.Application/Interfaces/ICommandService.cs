using Microsoft.Extensions.DependencyInjection;

namespace FeedbackBot.Application.Interfaces;

public interface ICommandsService
{
    public ICommand GetCommandInstance(IServiceScope scope, string commandTypeName);
    public string? GetCommandTypeNameBySlash(string slash);
}

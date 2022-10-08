using Microsoft.Extensions.DependencyInjection;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Common.Services;
using FeedbackBot.Application.Models.DTOs;
using Mapster;
using FeedbackBot.Application.Models.Resources;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Reflection;
using FeedbackBot.Application.Models.Contexts;

namespace FeedbackBot.Application.Extensions;

public static class ServiceCollectionExtensions
{
    private const string CommandResourcesPathTemplate = "Resources/Commands/{0}.json";
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
            .AddMemoryCache()
            .AddMapsterConfiguration()
            .AddCommandsAndResources()
            .AddScoped<IUpdateHandler, UpdateHandler>()
            .AddSingleton<ICommandsService, CommandsService>()
            .AddScoped<IResourcesService, ResourcesService>()
            .AddScoped<BehaviorContext>()
            .AddScoped<CommandContext>();

    private static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig<Telegram.Bot.Types.Message, MessageDto>.NewConfig()
            .Map(
                destination => destination.Sender,
                source => source.From == null ? null
                    : source.From.Adapt<UserDto>())
            .Map(
                destination => destination.ReplyTarget,
                source => source.ReplyToMessage == null ? null
                    : source.ReplyToMessage.Adapt<MessageDto>());

        return services;
    }

    private static IServiceCollection AddCommandsAndResources(this IServiceCollection services)
    {
        var commandTypes = GetImplementationsOf<ICommand>().ToArray();
        foreach (var commandType in commandTypes)
        {
            Log.Information("Registering command of type {0}", commandType.FullName);
            services.AddScoped(commandType);
        }

        return services.AddCommandResources(commandTypes.Select(x => x.FullName!));
    }

    private static IServiceCollection AddCommandResources(
    this IServiceCollection services, IEnumerable<string> commandTypeNames)
    {
        var resourceMap = commandTypeNames.ToDictionary(
            commandTypeName => commandTypeName, commandTypeName =>
            {
                var path = string.Format(
                    CommandResourcesPathTemplate, commandTypeName.Split('.').Last());

                var data = ParseJObjectFromRelativeLocation(path);
                return data is not null
                    ? new CommandResources(ParseJObjectFromRelativeLocation(path))
                    : null;
            });

        return services.AddSingleton<IDictionary<string, CommandResources>>(resourceMap!);
    }

    private static JObject? ParseJObjectFromRelativeLocation(string relativePath)
    {
        var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

        if (!File.Exists(absolutePath))
            return null;

        var json = File.ReadAllText(absolutePath);
        return JObject.Parse(json);
    }
    private static IEnumerable<Type> GetImplementationsOf<T>() where T : class =>
    Assembly.GetExecutingAssembly().GetTypes()
        .Where(x => x.GetInterfaces().Contains(typeof(T)));
}
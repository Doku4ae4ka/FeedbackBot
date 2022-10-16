using Microsoft.Extensions.DependencyInjection;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Common.Services;
using FeedbackBot.Application.Models.DTOs;
using Mapster;
using FeedbackBot.Application.Models.Resources;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Reflection;
using FeedbackBot.Application.Behaviors;
using FeedbackBot.Application.Models.Contexts;

namespace FeedbackBot.Application.Extensions;

public static class ServiceCollectionExtensions
{
    private const string CommandResourcesPathTemplate = "Resources/Commands/{0}.json";
    private const string BehaviorResourcesPathTemplate = "Resources/Behaviors/{0}.json";
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
            .AddMemoryCache()
            .AddMapsterConfiguration()
            .AddBehaviorsAndResources()
            .AddCommandsAndResources()
            .AddScoped<IUpdateHandler, UpdateHandler>()
            .AddSingleton<IInteractionService, InteractionService>()
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

    private static IServiceCollection AddBehaviorsAndResources(this IServiceCollection services)
    {
        // Register behaviors manually to preserve their order
        var implementations = new[]
        {
            typeof(ErrorBehavior),
            typeof(SlashCommandBehavior),
            typeof(MisunderstandingBehavior)
        };

        foreach (var implementation in implementations)
            services.AddScoped(typeof(IBehavior), implementation);

        var behaviorTypes = GetImplementationsOf<IBehavior>();
        foreach (var behaviorType in behaviorTypes)
        {
            if (!behaviorType.IsAbstract && services.All(x => x.ImplementationType != behaviorType))
                Log.Warning("Behavior of type {0} is not registered", behaviorType.FullName);
        }

        return services.AddBehaviorResources(implementations.Select(x => x.FullName!));
    }
    
    private static IServiceCollection AddBehaviorResources(
        this IServiceCollection services, IEnumerable<string> behaviorTypeNames)
    {
        var resourceMap = behaviorTypeNames.ToDictionary(
            behaviorTypeName => behaviorTypeName, behaviorTypeName =>
            {
                var path = string.Format(
                    BehaviorResourcesPathTemplate, behaviorTypeName.Split('.').Last());

                var data = ParseJObjectFromRelativeLocation(path);
                return data is not null
                    ? new BehaviorResources(data)
                    : null;
            });

        return services.AddSingleton<IDictionary<string, BehaviorResources?>>(resourceMap);
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
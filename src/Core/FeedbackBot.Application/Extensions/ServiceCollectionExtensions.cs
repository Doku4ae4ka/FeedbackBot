using Microsoft.Extensions.DependencyInjection;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Services;
using FeedbackBot.Application.Models.DTOs;
using Mapster;
using FeedbackBot.Application.Models.Resources;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Reflection;
using FeedbackBot.Application.Behaviors;
using FeedbackBot.Application.Models.Contexts;
using Microsoft.Extensions.Configuration;
using Radzinsky.Application.Services;

namespace FeedbackBot.Application.Extensions;

public static class ServiceCollectionExtensions
{
    private const string CommandResourcesPathTemplate = "Resources/Commands/{0}.json";
    private const string BehaviorResourcesPathTemplate = "Resources/Behaviors/{0}.json";

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddMemoryCache()
            .AddMapsterConfiguration(configuration)
            .AddBehaviorsAndResources()
            .AddCommandsAndResources()
            .AddCallbackQueryHandlers()
            .AddScoped<IUpdateHandler, UpdateHandler>()
            .AddScoped<IResourcesService, ResourcesService>()
            .AddTransient<IRuntimeInfoService, RuntimeInfoService>()
            .AddSingleton<ICheckpointMemoryService, CheckpointMemoryService>()
            .AddSingleton<ICommandsService, CommandsService>()
            .AddSingleton<IHashingService, Md5HashingService>()
            .AddSingleton<IReplyMemoryService, ReplyMemoryService>()
            .AddScoped<IStateService, StateService>()
            .AddScoped<BehaviorContext>()
            .AddScoped<CommandContext>()
            .AddScoped<CallbackQueryContext>();

    private static IServiceCollection AddMapsterConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var keyLength = configuration.GetValue<int>("Callbacks:CallbackHandlerKeyLength");
        
        TypeAdapterConfig<Telegram.Bot.Types.Update, UpdateDto>.NewConfig()
            .Map(
                destination => destination.CallbackQuery,
                source => source.CallbackQuery == null
                    ? null
                    : source.CallbackQuery.Adapt<CallbackQueryDto>());

        TypeAdapterConfig<Telegram.Bot.Types.CallbackQuery, CallbackQueryDto>.NewConfig()
            .Map(
                destination => destination.CallbackHandlerTypeNameHash,
                source => source.Data == null
                    ? null
                    : source.Data.Substring(0, keyLength))
            .Map(
                destination => destination.Data,
                source => source.Data == null
                    ? null
                    : source.Data.Substring(keyLength))
            .Map(
                destination => destination.Sender,
                source => source.From.Adapt<UserDto>());
        
        TypeAdapterConfig<Telegram.Bot.Types.Message, MessageDto>.NewConfig()
            .Map(
                destination => destination.Id,
                source => source.MessageId)
            .Map(
                destination => destination.Sender,
                source => source.From == null
                    ? null
                    : source.From.Adapt<UserDto>())
            .Map(
                destination => destination.ReplyTarget,
                source => source.ReplyToMessage == null
                    ? null
                    : source.ReplyToMessage.Adapt<MessageDto>());

        return services;
    }

    private static IServiceCollection AddBehaviorsAndResources(this IServiceCollection services)
    {
        // Register behaviors manually to preserve their order
        var implementations = new[]
        {
            typeof(ErrorBehavior),
            typeof(CallbackQueryBehavior),
            typeof(SlashCommandBehavior),
            typeof(MisunderstandingBehavior)
        };

        foreach (var implementation in implementations)
        {
            services.AddScoped(typeof(IBehavior), implementation);
            services.AddScoped(implementation, implementation);
        }

        var behaviorTypes = GetImplementationsOf<IBehavior>();
        foreach (var behaviorType in behaviorTypes)
        {
            if (!behaviorType.IsAbstract && services.All(x => x.ImplementationType != behaviorType))
                Log.Warning("Behavior of type {0} is not registered", behaviorType.FullName);
        }

        return services.AddBehaviorResources(implementations.Select(x => x.FullName!));
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

    private static IServiceCollection AddCallbackQueryHandlers(this IServiceCollection services)
    {
        var handlerTypes = GetImplementationsOf<ICallbackQueryHandler>().ToArray();
        foreach (var handlerType in handlerTypes)
        {
            Log.Information("Registering callback query handler of type {0}", handlerType.FullName);
            services.AddScoped(typeof(ICallbackQueryHandler), handlerType);
        }

        return services;
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
                    ? new CommandResources(data)
                    : null;
            })
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);;

        return services.AddSingleton<IDictionary<string, CommandResources?>>(resourceMap);
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
            })
            .Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value);

        return services.AddSingleton<IDictionary<string, BehaviorResources?>>(resourceMap);
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
﻿using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Checkpoints;
using FeedbackBot.Application.Models.Contexts;
using FeedbackBot.Application.Models.DTOs;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FeedbackBot.Application.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly IEnumerable<IBehavior> _behaviors;
    private readonly IResourcesService _resources;
    private readonly ITelegramBotClient _bot;
    private readonly BehaviorContext _behaviorContext;

    public UpdateHandler(
        IEnumerable<IBehavior> behaviors,
        IResourcesService resources,
        ITelegramBotClient bot,
        BehaviorContext behaviorContext)
    {
        _behaviors = behaviors;
        _resources = resources;
        _bot = bot;
        _behaviorContext = behaviorContext;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
#if DEBUG
        Log.Debug("Received update: {@0}", update);
#else
        Log.Information("Received message ({0}) from chat {1}: {2}",
            update.Message.MessageId, update.Message.Chat.Id, update.Message.Text);
#endif

        FillBehaviorContext(_behaviorContext, update);

        var enumerator = _behaviors.GetEnumerator();
        await RunNextBehaviorAsync(_behaviorContext);

        async Task RunNextBehaviorAsync(BehaviorContext context)
        {
            var previousBehaviorTypeName = context.HandlerTypeName;
            var previousResources = context.Resources;

            if (enumerator.MoveNext())
            {
                context.HandlerTypeName = enumerator.Current.GetType().FullName!;
                context.Resources =
                    _resources.GetBehaviorResources(context.HandlerTypeName);

                Log.Debug("Entering behavior {0}", context.HandlerTypeName);

                try
                {
                    await enumerator.Current.HandleAsync(context, RunNextBehaviorAsync);
                }
                finally
                {
                    Log.Debug("Leaving behavior {0}", context.HandlerTypeName);

                    context.HandlerTypeName = previousBehaviorTypeName;
                    context.Resources = previousResources;
                }
            }
        }
    }

    private void FillBehaviorContext(BehaviorContext context, Update update)
    {
        context.Update = update.Adapt<UpdateDto>();

        if (update.Message is not null)
        {
            context.Update.Message.IsReplyToMe = context.Update.Message.ReplyTarget?.Sender.Id == _bot.BotId;
            context.Update.Message.IsPrivate = update.Message!.Chat.Type == ChatType.Private;
        }

        context.Update.CallbackQuery = update.CallbackQuery?.Adapt<CallbackQueryDto>();
    }
}
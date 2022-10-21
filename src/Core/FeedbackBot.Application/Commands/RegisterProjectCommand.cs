using FeedbackBot.Application.Delegates;
using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;
using FeedbackBot.Application.Models.DTOs;
using FeedbackBot.Application.Services;
using FeedbackBot.Domain.Models.Entities;
using FeedbackBot.Persistence;
using Mapster;
using Telegram.Bot.Types.ReplyMarkups;

namespace FeedbackBot.Application.Commands;

public class RegisterProjectCommand : ICommand, ICallbackQueryHandler
{
    private record RegisterProjectState(
        long RespondentUserId,
        string? ProjectData = null,
        string? Email = null);
    
    private readonly IStateService _states; 
    private readonly IHashingService _hasher;
    private readonly IResourcesService _resources;
    private readonly ApplicationDbContext _dbContext;
    
    public RegisterProjectCommand(IStateService states,
        IHashingService hasher,
        IResourcesService resources,
        ApplicationDbContext dbContext)
    {
        _states = states;
        _hasher = hasher;
        _resources = resources;
        _dbContext = dbContext;
    }
    public async Task HandleCallbackQueryAsync(CallbackQueryContext context, CancellationToken token)
    {
        ParseCallbackData(context.Query.Data, out var surveyId, out var callbackKey, out _);
        var stateKey = GetSurveyStateKey(surveyId);
        var state = await _states.ReadStateAsync<RegisterProjectState>(stateKey);

        if (state is null)
        {
            await context.ShowAlertAsync("Время регистрации истекло");
            return;
        }

        CallbackQueryContextHandler callbackQueryHandler = callbackKey switch
        {
            "ChoosingSchool" => HandleChoosingCallbackAsync,
            _ => throw new InvalidOperationException()
        };

        await callbackQueryHandler(context);
    }

    public async Task ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var checkpoint = context.GetCheckpoint();
        var stateKey = GetSurveyStateKey(context.Update.InteractorUserId!.Value);

        if (checkpoint?.HandlerTypeName.EndsWith("Command") ?? false)
            context.ResetCheckpoint();
        if (await _states.ReadStateAsync<RegisterProjectState>(stateKey) is null && checkpoint?.Name == null)
        {
            await context.SendTextAsync(context.Resources!.Get("DescribeProject"));
            context.SetCheckpoint("DescribeProject");
            return;
        }
        
        if (await _states.ReadStateAsync<RegisterProjectState>(stateKey) is not null)
        {
            await context.SendTextAsync("в данный момент вы уже регестрируете проект");
            return;
        }

        var state = new RegisterProjectState(context.Update.InteractorUserId!.Value);
        await _states.WriteStateAsync(stateKey, state with { ProjectData = context.Message.Text });
        
        
        await AskForChoosingAsync(context);
    }

    private async Task AskForChoosingAsync(CommandContext context)
    {
        var factory = new ButtonFactory<RegisterProjectCommand>(_hasher, $"{context.Update.InteractorUserId!.Value} ChoosingSchool {{0}}");

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new() { factory.CreateCallbackDataButton("Школа на Большом Казенном", "Email1") },
            new() { factory.CreateCallbackDataButton("Школа на Большом Трехсвятительском", "Email2") },
            new() { factory.CreateCallbackDataButton("Школа на Ляле", "Email3") }
        };

        await context.SendTextAsync(context.Resources!.Get("ChooseSchool"),
            replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    private async Task HandleChoosingCallbackAsync(CallbackQueryContext context)
    {
        ParseCallbackData(context.Query.Data, out var surveyId, out _, out var payload);

        var stateKey = GetSurveyStateKey(surveyId);
        var state = await _states.ReadStateAsync<RegisterProjectState>(stateKey);

        if (state!.Email is not null)
        {
            await context.ShowAlertAsync("Ты уже выбрал отделение!");
            return;
        }

        await _states.WriteStateAsync(stateKey, state with { Email = payload });

        await ProjectSentAsync(context);
    }

    private async Task ProjectSentAsync(CallbackQueryContext context)
    {
        var stateKey = GetSurveyStateKey(context.Update.InteractorUserId!.Value);
        var state = await _states.ReadStateAsync<RegisterProjectState>(stateKey);
        var checkpoint = context.GetCheckpoint();
        
        var request = new EmailRequestDto();
        var data = state.ProjectData.Split("\n");
        request.FullName = data[0];
        request.Partners = data[1];
        request.Grade = data[2];
        request.Profile = data[3];
        request.RequestSubject = data[4];
        request.RequestContent = data[5];
        request.Email = state.Email;
        
        await _dbContext.EmailRequests.AddAsync(request.Adapt<EmailRequest>());

        await _dbContext.SaveChangesAsync();
        
        await context.EditTextAsync(context.Query.Message.Id,_resources.GetCommandResources("FeedbackBot.Application.Commands.RegisterProjectCommand")!.Get("ProjectSent"));

        await _states.ResetStateAsync(stateKey);
    }

    private static void ParseCallbackData(
        string data,
        out long respondentUserId,
        out string callbackKey,
        out string payload)
    {
        var words = data.Split();
        respondentUserId = long.Parse(words[0]);
        callbackKey = words[1];
        payload = string.Join(' ', words[2..]);
    }

    private string GetSurveyStateKey(long respondentUserId) =>
        $"__RegisterProject__{respondentUserId}";
}
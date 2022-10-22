using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Contexts;
using FeedbackBot.Persistence;
using Telegram.Bot.Types.Enums;

namespace FeedbackBot.Application.Commands;

public class ReceiveEmailCommand
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _dbContext;
    
    public ReceiveEmailCommand(
        IEmailService emailService,
        ApplicationDbContext dbContext)
    {
        _emailService = emailService;
        _dbContext = dbContext;
    }
    
    public async Task ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var response = context.Resources!.Get("EmailReceived", context.Message.Sender.FirstName);
        await context.DeletePreviousReplyAsync();
        await context.SendTextAsync(response, ParseMode.Html);
        //await _emailService.ReceiveEmailAsync(project);
    }
}
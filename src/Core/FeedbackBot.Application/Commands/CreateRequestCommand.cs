using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Checkpoints;
using FeedbackBot.Application.Models.Contexts;
using FeedbackBot.Application.Models.DTOs;
using FeedbackBot.Domain.Models.Entities;
using FeedbackBot.Persistence;
using Mapster;

namespace FeedbackBot.Application.Commands;

public class CreateRequestCommand : ICommand
{
    private readonly ApplicationDbContext _dbContext;

    public CreateRequestCommand(ApplicationDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var checkpoint = context.GetCheckpoint();
        
        if (checkpoint?.HandlerTypeName.EndsWith("Command") ?? false)
            context.ResetCheckpoint();
        else if (checkpoint?.Name == null)
        {
            await context.SendTextAsync(context.Resources!.Get("DescribeRequest"));
            context.SetCheckpoint("DescribeRequest");
            return;
        }
        var data = context.Message.Text.Split('\n');
        var request = new EmailRequestDto();
        request.FullName = data[0];
        request.Partners = data[1];
        request.Grade = data[2];
        request.Profile = data[3];
        request.RequestSubject = data[4];
        request.RequestContent = data[5];
        request.Email = "Email";
        
        await _dbContext.EmailRequests.AddAsync(request.Adapt<EmailRequest>(),cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await context.SendTextAsync(context.Resources!.Get("RequestSent"));
    }
}
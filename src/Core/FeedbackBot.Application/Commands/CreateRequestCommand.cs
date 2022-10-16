using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Checkpoints;
using FeedbackBot.Application.Models.Contexts;
using FeedbackBot.Application.Models.DTOs;
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
        if (context.Checkpoint is CommandCheckpoint)
            context.ResetCheckpoint();
        else if (string.IsNullOrWhiteSpace(context.Payload))
        {
            await context.ReplyAsync(context.Resources!.Get("DescribeRequest"));
            context.SetCommandCheckpoint("DescribeRequest");
            return;
        }
        var data = context.Payload.Split('\n');
        var request = new EmailRequestDto();
        request.FullName = data[0];
        request.Grade = data[1];
        request.Profile = data[2];
        request.RequestSubject = data[3];
        request.RequestContent = data[4];

        await _dbContext.EmailRequests.AddAsync(data.Adapt<EmailRequest>);

        await _dbContext.SaveChangesAsync();
        await context.ReplyAsync(context.Resources!.Get("RequestSent"));
    }
}
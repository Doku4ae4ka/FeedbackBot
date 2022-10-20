namespace FeedbackBot.Application.Models.Checkpoints;

public record Checkpoint<TPayload>(string Name, string HandlerTypeName, long? ChatId, TPayload Payload)
    : Checkpoint(Name, HandlerTypeName, ChatId);
namespace FeedbackBot.Application.Models.Checkpoints;

public record Checkpoint(string Name, string HandlerTypeName, long? ChatId);
namespace FeedbackBot.Application.Models.Checkpoints;

public record MentionCheckpoint(string Name, TimeSpan Duration)
    : Checkpoint(Name, Duration);
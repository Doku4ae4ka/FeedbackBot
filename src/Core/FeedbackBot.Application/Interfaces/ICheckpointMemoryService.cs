using FeedbackBot.Application.Models.Checkpoints;

namespace FeedbackBot.Application.Interfaces;

public interface ICheckpointMemoryService
{
    public void SetCheckpoint(long userId, Checkpoint checkpoint, TimeSpan? duration = null);
    public Checkpoint? GetLocalCheckpoint(long userId, long chatId, string handlerTypeName);
    public void ResetCheckpoint(long userId);
    public Checkpoint? GetCheckpoint(long userId);
}
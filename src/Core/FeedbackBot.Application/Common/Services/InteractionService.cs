using FeedbackBot.Application.Interfaces;
using FeedbackBot.Application.Models.Checkpoints;
using Microsoft.Extensions.Caching.Memory;

namespace FeedbackBot.Application.Common.Services;

public class InteractionService : IInteractionService
{
    private readonly IMemoryCache _cache;

    public InteractionService(IMemoryCache cache) =>
        _cache = cache;

    public CommandCheckpoint IssueCheckpoint(string name, string handlerTypeName, long userId)
    {
        var checkpoint = new CommandCheckpoint(name, TimeSpan.FromMinutes(1), handlerTypeName);
        SetUserCheckpoint(userId, checkpoint);
        return checkpoint;
    }

    public Checkpoint? TryGetCurrentCheckpoint(long userId)
    {
        _cache.TryGetValue(GetCheckpointCacheEntryName(userId), out Checkpoint? checkpoint);
        return checkpoint;
    }

    public void ResetCheckpoint(long userId) =>
        _cache.Remove(GetCheckpointCacheEntryName(userId));

    public Task SetPreviousReplyMessageIdAsync(string handlerTypeName, long chatId, int messageId)
    {
        _cache.Set(GetReplyMessageIdCacheEntryName(handlerTypeName, chatId), messageId);
        return Task.CompletedTask;
    }

    public Task<int?> TryGetPreviousReplyMessageIdAsync(string handlerTypeName, long chatId)
    {
        _cache.TryGetValue(GetReplyMessageIdCacheEntryName(handlerTypeName, chatId), out int? messageId);
        return Task.FromResult(messageId);
    }

    private void SetUserCheckpoint(long userId, Checkpoint checkpoint) =>
        _cache.Set(GetCheckpointCacheEntryName(userId), checkpoint, checkpoint.Duration);

    private static string GetCheckpointCacheEntryName(long userId) => $"Checkpoint-{userId}";

    private static string GetReplyMessageIdCacheEntryName(string commandTypeName, long chatId) =>
        $"Reply-{commandTypeName}-{chatId}";
}
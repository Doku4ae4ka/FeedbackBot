﻿using FeedbackBot.Application.Interfaces;
using FeedbackBot.Domain.Models.Entities;
using FeedbackBot.Persistence;
using Newtonsoft.Json;

namespace FeedbackBot.Application.Services;

public class StateService : IStateService
{
    private readonly ApplicationDbContext _dbContext;

    public StateService(ApplicationDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<T?> ReadStateAsync<T>(string key) where T : class
    {
        var entry = await FindEntryAsync(key);
        return entry is not null
            ? JsonConvert.DeserializeObject<T>(entry.Payload)
            : null;
    }

    public async Task WriteStateAsync(string key, object payload)
    {
        var entry = await FindEntryAsync(key);
        var serializedPayload = JsonConvert.SerializeObject(payload);

        if (entry is null)
            await _dbContext.States.AddAsync(new State(key, serializedPayload));
        else
            entry.Payload = serializedPayload;

        await _dbContext.SaveChangesAsync();
    }

    public async Task ResetStateAsync(string key)
    {
        var entry = await FindEntryAsync(key);
        
        if (entry is not null)
            _dbContext.States.Remove(entry);

        await _dbContext.SaveChangesAsync();
    }

    private async Task<State?> FindEntryAsync(string key) =>
        await _dbContext.States.FindAsync(key);
}
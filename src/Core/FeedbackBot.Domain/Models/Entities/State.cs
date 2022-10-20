using System.ComponentModel.DataAnnotations;

namespace FeedbackBot.Domain.Models.Entities;

public class State
{
    [Key] public string Key { get; set; }
    public string Payload { get; set; }

    private State() { }

    public State(string key, string payload)
    {
        Key = key;
        Payload = payload;
    }
}
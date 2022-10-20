namespace FeedbackBot.Application.Interfaces;

public interface IRuntimeInfoService
{
    public TimeSpan GetUptime();
}
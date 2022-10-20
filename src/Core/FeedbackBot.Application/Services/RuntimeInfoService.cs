using System.Diagnostics;
using FeedbackBot.Application.Interfaces;

namespace FeedbackBot.Application.Services;

public class RuntimeInfoService : IRuntimeInfoService
{
    public TimeSpan GetUptime()
    {
        using var process = Process.GetCurrentProcess();
        return DateTime.UtcNow - process.StartTime.ToUniversalTime();
    }
}
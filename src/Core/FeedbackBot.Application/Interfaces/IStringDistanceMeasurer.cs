namespace FeedbackBot.Application.Interfaces;

public interface IStringDistanceMeasurer
{
    public int MeasureDistance(string a, string b);
}
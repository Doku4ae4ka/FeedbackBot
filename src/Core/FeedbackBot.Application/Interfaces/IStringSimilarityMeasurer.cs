
using FeedbackBot.Application.Enumerations;

namespace FeedbackBot.Application.Interfaces;

public interface IStringSimilarityMeasurer
{
    public StringSimilarity MeasureSimilarity(string a, string b);
}
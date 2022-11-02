using Newtonsoft.Json.Linq;

namespace FeedbackBot.Application.Models.Resources;

public class CommonResources : ResourcesBase
{
    public CommonResources(JObject data)
        : base(data) { }
}
using Newtonsoft.Json.Linq;

namespace FeedbackBot.Application.Models.Resources;

public class BehaviorResources : ResourcesBase
{
    public BehaviorResources(JObject data)
        : base(data) { }
}
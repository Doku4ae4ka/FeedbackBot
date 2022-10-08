using Newtonsoft.Json.Linq;

namespace FeedbackBot.Application.Models.Resources;

public class BehaviorResources : Resources
{
    public BehaviorResources(JObject data)
        : base(data) { }
}

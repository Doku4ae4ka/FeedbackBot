using Newtonsoft.Json.Linq;

namespace FeedbackBot.Application.Models.Resources;

public class CommandResources : Resources
{
    public CommandResources(JObject data)
        : base(data) { }

    public IEnumerable<string> Slashes =>
    GetManyOrEmpty<string>("Slashes")!;
}

using Newtonsoft.Json.Linq;
using System.Globalization;

namespace FeedbackBot.Application.Models.Resources;

public class Resources
{
    private readonly JObject _data;

    public Resources(JObject data) =>
        _data = data;
    public IEnumerable<string> GetMany(string key, params object[] args) =>
        GetMany<string>(key)
            .Select(x => string.Format(CultureInfo.InvariantCulture, x, args));

    public IEnumerable<T> GetMany<T>(string key) =>
        _data.GetValue(key)!.Values<T>()!;

    public IEnumerable<T?> GetManyOrEmpty<T>(string key) =>
        _data.GetValue(key)?.Values<T>() ?? Enumerable.Empty<T>();

    public string Get(string key, params object[] args) =>
        string.Format(CultureInfo.InvariantCulture, Get<string>(key), args);

    public T Get<T>(string key) =>
        _data.GetValue(key)!.Value<T>()!;
}

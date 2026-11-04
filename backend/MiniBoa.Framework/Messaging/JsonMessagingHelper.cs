using System.Reflection;
using System.Text.Json;

namespace MiniBoa.Framework.Messaging;

/// <summary>
/// Mesajları dll içine gömülü Resources/Messages.{dil}.json dosyalarından okur.
/// Dosyalar bir kez yüklenir; dil isteğin RequestContext'inden alınır.
/// </summary>
public sealed class JsonMessagingHelper(RequestContext context) : IMessagingHelper
{
    private const string DefaultLanguage = "tr";

    private static readonly Lazy<Dictionary<string, Dictionary<string, Dictionary<string, string>>>> Catalogs =
        new(LoadCatalogs);

    public string GetMessage(string category, string key)
    {
        return Find(context.Language, category, key)
               ?? Find(DefaultLanguage, category, key)
               ?? $"{category}.{key}";
    }

    private static string? Find(string language, string category, string key)
    {
        if (Catalogs.Value.TryGetValue(language, out var categories)
            && categories.TryGetValue(category, out var messages)
            && messages.TryGetValue(key, out var message))
        {
            return message;
        }

        return null;
    }

    private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> LoadCatalogs()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var catalogs = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        foreach (var resourceName in assembly.GetManifestResourceNames().Where(n => n.EndsWith(".json")))
        {
            // Örn: MiniBoa.Framework.Resources.Messages.tr.json -> "tr"
            var language = resourceName.Split('.')[^2];

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            catalogs[language] =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream) ?? [];
        }

        return catalogs;
    }
}

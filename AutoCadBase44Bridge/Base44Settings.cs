using Microsoft.Win32;

namespace AutoCadBase44Bridge;

internal sealed record Base44Settings(string WebhookUrl, string ApiKey)
{
    private const string RegistryPath = @"Software\AutoCadBase44Bridge";

    public static Base44Settings Load()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
        return new Base44Settings(
            key?.GetValue("WebhookUrl") as string ?? string.Empty,
            key?.GetValue("ApiKey") as string ?? string.Empty);
    }

    public static void Save(Base44Settings settings)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
        key.SetValue("WebhookUrl", settings.WebhookUrl);
        key.SetValue("ApiKey", settings.ApiKey);
    }
}

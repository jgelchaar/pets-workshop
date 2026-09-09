using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace AutoCadBase44Bridge;

public sealed class Base44Commands
{
    [CommandMethod("BASE44CONFIG")]
    public void Configure()
    {
        var editor = Application.DocumentManager.MdiActiveDocument?.Editor;
        if (editor is null)
        {
            return;
        }

        var current = Base44Settings.Load();
        var endpoint = PromptForValue(editor, "Base44 webhook URL", current.WebhookUrl);
        if (endpoint is null)
        {
            return;
        }

        var apiKey = PromptForValue(editor, "Base44 API key (leave blank to keep current)", string.Empty);
        if (apiKey is null)
        {
            return;
        }

        Base44Settings.Save(new Base44Settings(
            endpoint,
            string.IsNullOrWhiteSpace(apiKey) ? current.ApiKey : apiKey));
        editor.WriteMessage("\nBase44 settings saved for the current Windows user.");
    }

    [CommandMethod("BASE44SEND")]
    public void SendSelection()
    {
        var document = Application.DocumentManager.MdiActiveDocument;
        if (document is null)
        {
            return;
        }

        var editor = document.Editor;
        var selection = editor.GetSelection();
        if (selection.Status != PromptStatus.OK)
        {
            editor.WriteMessage("\nNo objects selected.");
            return;
        }

        var settings = Base44Settings.Load();
        if (string.IsNullOrWhiteSpace(settings.WebhookUrl))
        {
            editor.WriteMessage("\nRun BASE44CONFIG before sending data.");
            return;
        }

        try
        {
            var payload = DrawingPayload.Create(document, selection.Value);
            Base44Client.Send(settings, payload);
            editor.WriteMessage($"\nSent {payload.Objects.Count} object(s) to Base44.");
        }
        catch (System.Exception exception)
        {
            editor.WriteMessage($"\nBase44 send failed: {exception.Message}");
        }
    }

    private static string? PromptForValue(Editor editor, string label, string defaultValue)
    {
        var prompt = new PromptStringOptions($"\n{label} [{defaultValue}]: ")
        {
            AllowSpaces = true,
            UseDefaultValue = true,
            DefaultValue = defaultValue
        };
        var result = editor.GetString(prompt);
        return result.Status == PromptStatus.OK ? result.StringResult : null;
    }
}

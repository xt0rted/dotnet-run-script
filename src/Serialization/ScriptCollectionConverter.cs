namespace RunScript.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

internal class ScriptCollectionConverter : JsonConverter<ScriptCollection>
{
    public override ScriptCollection Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var scripts = new ScriptCollection();

        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        using (var jsonScripts = jsonDoc.RootElement.EnumerateObject())
        {
            foreach (var script in jsonScripts)
            {
                scripts.Add(script.Name, script.Value.ToString());
            }
        }

        return scripts;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ScriptCollection value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var element in value)
        {
            writer.WritePropertyName(element.Name);
            writer.WriteStringValue(element.Script);
        }

        writer.WriteEndObject();
    }
}

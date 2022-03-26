namespace RunScript.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

internal class CaseInsensitiveDictionaryConverter<TValue> : JsonConverter<Dictionary<string, TValue>>
{
    public override Dictionary<string, TValue> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var dict = (Dictionary<string, TValue>)JsonSerializer
            .Deserialize(ref reader, typeToConvert, options)!;

#pragma warning disable CA1308 // Normalize strings to uppercase
        return dict.ToDictionary(
            i => i.Key.ToLowerInvariant(),
            i => i.Value,
            StringComparer.OrdinalIgnoreCase);
#pragma warning restore CA1308 // Normalize strings to uppercase
    }

    public override void Write(
        Utf8JsonWriter writer,
        Dictionary<string, TValue> value,
        JsonSerializerOptions options)
        => JsonSerializer.Serialize(
            writer,
            value,
            value.GetType(),
            options);
}

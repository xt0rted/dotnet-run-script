namespace RunScript;

public class ScriptCollectionConverter : WriteOnlyJsonConverter<ScriptCollection>
{
    public override void Write(VerifyJsonWriter writer, ScriptCollection value)
    {
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (value is null) throw new ArgumentNullException(nameof(value));

        writer.WriteStartObject();

        foreach (var (name, script) in value)
        {
            writer.WritePropertyName(name);
            writer.WriteValue(script);
        }

        writer.WriteEndObject();
    }
}

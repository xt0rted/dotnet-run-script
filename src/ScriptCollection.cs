namespace RunScript;

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

using RunScript.Serialization;

[JsonConverter(typeof(ScriptCollectionConverter))]
public class ScriptCollection : KeyedCollection<string, ScriptMapping>
{
    public ScriptCollection()
        : base(StringComparer.Ordinal)
    {
    }

    protected override string GetKeyForItem(ScriptMapping item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));

        return item.Name;
    }

    public void Add(string name, string script)
        => Add(new ScriptMapping(name, script));
}

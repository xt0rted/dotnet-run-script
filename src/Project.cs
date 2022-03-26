namespace RunScript;

using System.Text.Json.Serialization;

using RunScript.Serialization;

public class Project
{
    public string? ScriptShell { get; set; }

    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string?>))]
    public Dictionary<string, string?>? Scripts { get; set; }
}

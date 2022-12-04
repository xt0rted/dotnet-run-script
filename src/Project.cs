namespace RunScript;

using System.Text.Json.Serialization;

public class Project
{
    [JsonPropertyName("scriptShell")]
    public string? ScriptShell { get; set; }

    [JsonPropertyName("scripts")]
    public ScriptCollection? Scripts { get; set; }
}

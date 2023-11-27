namespace RunScript;

using System.Text.Json;

internal static class ProjectLoader
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public static async Task<(Project, string)> LoadAsync(string executingDirectory)
    {
        var jsonPath = CheckFolderForFile(executingDirectory, "global.json");

        if (jsonPath is null)
        {
            throw new RunScriptException("No global.json found in folder path");
        }

        var rootPath = Path.GetDirectoryName(jsonPath)!;
        var workingDirectory = rootPath != executingDirectory
            ? rootPath
            : executingDirectory;

        var project = await LoadGlobalJsonAsync(jsonPath);

        if (project is null)
        {
            throw new RunScriptException("Error parsing global.json");
        }

        if (project.Scripts is null || project.Scripts.Count == 0)
        {
            throw new RunScriptException("No scripts found in the global.json");
        }

        return (project, workingDirectory);
    }

    private static string? CheckFolderForFile(string path, string file)
    {
        var filePath = Path.Combine(path, file);

        if (File.Exists(filePath))
        {
            return filePath;
        }

        var parentPath = Directory.GetParent(path)?.FullName;

        if (parentPath is null)
        {
            return null;
        }

        return CheckFolderForFile(parentPath, file);
    }

    private static async Task<Project?> LoadGlobalJsonAsync(string jsonPath)
    {
        var json = await File.ReadAllTextAsync(jsonPath);

        try
        {
            return JsonSerializer.Deserialize<Project>(
                json,
                _jsonOptions);
        }
        catch
        {
            return null;
        }
    }
}

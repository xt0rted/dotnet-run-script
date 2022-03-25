namespace RunScript;

using System.Text.Json;

public class ProjectLoader
{
    public async Task<(Project, Dictionary<string, string?>, string)> LoadAsync(string executingDirectory)
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

        var scripts = LoadScripts(project);
        if (scripts is null)
        {
            throw new RunScriptException("Error loading scripts");
        }

        return (project, scripts, workingDirectory);
    }

    private string? CheckFolderForFile(string path, string file)
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
            var globalJson = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            });

            return globalJson;
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, string?> LoadScripts(Project project)
    {
        if (project is null) throw new ArgumentNullException(nameof(project));

        if (project.Scripts is null || project.Scripts.Count == 0)
        {
            throw new RunScriptException("No scripts found in the global.json");
        }

        return new Dictionary<string, string?>(project.Scripts, StringComparer.OrdinalIgnoreCase);
    }
}

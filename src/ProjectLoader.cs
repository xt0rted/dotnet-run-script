namespace RunScript;

using System.Text.Json;

public class ProjectLoader
{
    public async Task<(Project, Dictionary<string, string?>, string)> Load(string executingDirectory)
    {
        var jsonPath = CheckFolderForFile(executingDirectory, "global.json");
        if (jsonPath == null)
        {
            throw new RunScriptException("No global.json found in folder path");
        }

        var rootPath = Path.GetDirectoryName(jsonPath)!;
        var workingDirectory = rootPath != executingDirectory
            ? rootPath
            : executingDirectory;

        var project = await LoadGlobalJson(jsonPath);
        if (project == null)
        {
            throw new RunScriptException("Error parsing global.json");
        }

        var scripts = LoadScripts(project);
        if (scripts == null)
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
        if (parentPath == null)
        {
            return null;
        }

        return CheckFolderForFile(parentPath, file);
    }

    private static async Task<Project?> LoadGlobalJson(string jsonPath)
    {
        var json = await File.ReadAllTextAsync(jsonPath);

        var globalJson = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        });

        return globalJson;
    }

    private static Dictionary<string, string?> LoadScripts(Project project)
    {
        if (project is null) throw new ArgumentNullException(nameof(project));

        if (project.Scripts == null || project.Scripts.Count == 0)
        {
            throw new RunScriptException("No scripts found in the global.json");
        }

        var scripts = new Dictionary<string, string?>(project.Scripts, StringComparer.OrdinalIgnoreCase);

        return scripts;
    }
}

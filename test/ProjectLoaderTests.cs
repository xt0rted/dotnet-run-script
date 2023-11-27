namespace RunScript;

[Trait("category", "unit")]
[UsesVerify]
public class ProjectLoaderTests
{
    [Fact]
    public void Should_throw_if_no_globaljson_found_in_tree()
    {
        // Given / When
        var action = () => ProjectLoader.LoadAsync(Path.GetTempPath());

        // Then
        var ex = action.ShouldThrow<RunScriptException>();
        ex.Message.ShouldBe("No global.json found in folder path");
    }

    [Fact]
    public void Should_throw_if_malformed_file()
    {
        // Given
        var testPath = TestPath("malformed");

        // When
        var action = () => ProjectLoader.LoadAsync(testPath);

        // Then
        var ex = action.ShouldThrow<RunScriptException>();
        ex.Message.ShouldBe("Error parsing global.json");
    }

    [Fact]
    public void Should_throw_if_no_scripts_found()
    {
        // Given
        var testPath = TestPath("no-scripts");

        // When
        var action = () => ProjectLoader.LoadAsync(testPath);

        // Then
        var ex = action.ShouldThrow<RunScriptException>();
        ex.Message.ShouldBe("No scripts found in the global.json");
    }

    [Fact]
    public async Task Should_find_in_root()
    {
        // Given
        var testPath = TestPath("dir1");

        // When
        var (project, workingDirectory) = await ProjectLoader.LoadAsync(testPath);

        // Then
        project.ShouldNotBeNull();

        await Verify(project);

        workingDirectory.ShouldBe(TestPath("dir1"));
    }

    [Fact]
    public async Task Should_look_up_the_tree()
    {
        // Given
        var testPath = TestPath("dir1", "dir2", "dir3");

        // When
        var (project, workingDirectory) = await ProjectLoader.LoadAsync(testPath);

        // Then
        project.ShouldNotBeNull();

        await Verify(project);

        workingDirectory.ShouldBe(TestPath("dir1"));
    }

    [Fact]
    public async Task Should_treat_script_names_as_lowercase()
    {
        // Given
        var testPath = TestPath("script-names");

        // When
        var (project, _) = await ProjectLoader.LoadAsync(testPath);

        // Then
        project.Scripts?.Comparer.ShouldBe(StringComparer.OrdinalIgnoreCase);

        await Verify(project);
    }

    private string TestPath(params string[] folders)
    {
        return Path.Join(segments().ToArray());

        IEnumerable<string> segments()
        {
            yield return AttributeReader.GetProjectDirectory(GetType().Assembly);
            yield return "test-configs";

            foreach (var folder in folders)
            {
                yield return folder;
            }
        }
    }
}

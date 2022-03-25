namespace RunScript;

[UsesVerify]
public class ProjectLoaderTests
{
    [Fact]
    public void Should_throw_if_no_globaljson_found_in_tree()
    {
        // Given
        var projectLoader = new ProjectLoader();

        // When
        var action = async () => await projectLoader.LoadAsync(Path.GetTempPath());

        // Then
        var ex = action.ShouldThrow<RunScriptException>();
        ex.Message.ShouldBe("No global.json found in folder path");
    }

    [Fact]
    public void Should_throw_if_malformed_file()
    {
        // Given
        var testPath = TestPath("malformed");
        var projectLoader = new ProjectLoader();

        // When
        var action = async () => await projectLoader.LoadAsync(testPath);

        // Then
        var ex = action.ShouldThrow<RunScriptException>();
        ex.Message.ShouldBe("Error parsing global.json");
    }

    [Fact]
    public void Should_throw_if_no_scripts_found()
    {
        // Given
        var testPath = TestPath("no-scripts");
        var projectLoader = new ProjectLoader();

        // When
        var action = async () => await projectLoader.LoadAsync(testPath);

        // Then
        var ex = action.ShouldThrow<RunScriptException>();
        ex.Message.ShouldBe("No scripts found in the global.json");
    }

    [Fact]
    public async Task Should_find_in_root()
    {
        // Given
        var testPath = TestPath("dir1");
        var projectLoader = new ProjectLoader();

        // When
        var (project, scripts, workingDirectory) = await projectLoader.LoadAsync(testPath);

        // Then
        project.ShouldNotBeNull();

        await Verify(project);

        scripts.ShouldBeEquivalentTo(
            new Dictionary<string, string>
            {
                { "test", "echo \"dir1\" && exit 1" },
            });

        workingDirectory.ShouldBe(TestPath("dir1"));
    }

    [Fact]
    public async Task Should_look_up_the_tree()
    {
        // Given
        var testPath = TestPath("dir1", "dir2", "dir3");
        var projectLoader = new ProjectLoader();

        // When
        var (project, scripts, workingDirectory) = await projectLoader.LoadAsync(testPath);

        // Then
        project.ShouldNotBeNull();

        await Verify(project);

        scripts.ShouldBeEquivalentTo(
            new Dictionary<string, string>
            {
                { "test", "echo \"dir1\" && exit 1" },
            });
        workingDirectory.ShouldBe(TestPath("dir1"));
    }

    [Fact]
    public async Task Should_treat_script_names_as_lowercase()
    {
        // Given
        var testPath = TestPath("script-names");
        var projectLoader = new ProjectLoader();

        // When
        var (project, scripts, _) = await projectLoader.LoadAsync(testPath);

        // Then
        project.Scripts.ShouldNotBeNull();

        project.Scripts["bUiLD"].ShouldBe("build");
        project.Scripts["TEST"].ShouldBe("test");
        project.Scripts["pack"].ShouldBe("pack");

        await Verify(project);

        scripts["build"].ShouldBe("build");
        scripts["test"].ShouldBe("test");
        scripts["pack"].ShouldBe("pack");
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

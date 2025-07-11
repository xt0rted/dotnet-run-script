namespace RunScript;

using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Threading.Tasks;

[Trait("category", "unit")]
public class CommandGroupRunnerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_a_single_script(bool isWindows)
    {
        // Given
        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(999),
        };

        var (_, groupRunner) = SetUpTest(commandRunners, isWindows);

        // When
        var result = await groupRunner.RunAsync("clean", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedOnceExactly();

        await Verify(commandRunners).UseParameters(isWindows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_with_a_pre_script(bool isWindows)
    {
        // Given
        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(0),
            new(999),
        };

        var (_, groupRunner) = SetUpTest(commandRunners, isWindows);

        // When
        var result = await groupRunner.RunAsync("build", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedTwiceExactly();

        await Verify(commandRunners).UseParameters(isWindows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_with_a_post_script(bool isWindows)
    {
        // Given
        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(0),
            new(999),
        };

        var (_, groupRunner) = SetUpTest(commandRunners, isWindows);

        // When
        var result = await groupRunner.RunAsync("test", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedTwiceExactly();

        await Verify(commandRunners).UseParameters(isWindows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_with_a_pre_and_post_script(bool isWindows)
    {
        // Given
        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(0),
            new(0),
            new(999),
        };

        var (_, groupRunner) = SetUpTest(commandRunners, isWindows);

        // When
        var result = await groupRunner.RunAsync("pack", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappened(3, Times.Exactly);

        await Verify(commandRunners).UseParameters(isWindows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_emit_environment_variable_list_when_env_script_not_defined(bool isWindows)
    {
        // Given
        TestCommandRunner[] commandRunners =
        {
            new(999),
        };

        var (output, groupRunner) = SetUpTest(commandRunners, isWindows);

        // When
        var result = await groupRunner.RunAsync("env", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustNotHaveHappened();

        await Verify(output).UseParameters(isWindows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_emit_environment_variable_list_and_pre_and_post_scripts_when_env_script_not_defined(bool isWindows)
    {
        // Given
        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(0),
            new(999),
        };

        var (output, groupRunner) = SetUpTest(
            commandRunners,
            isWindows,
            scripts =>
            {
                scripts.Add("preenv", "echo preenv");
                scripts.Add("postenv", "echo postenv");
            });

        // When
        var result = await groupRunner.RunAsync("env", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedTwiceExactly();

        await Verify(
            new
            {
                output,
                commandRunners,
            })
            .UseParameters(isWindows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_env_script_if_defined(bool isWindows)
    {
        // Given
        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(999),
        };

        var (output, groupRunner) = SetUpTest(
            commandRunners,
            isWindows,
            scripts => scripts.Add("env", "echo env"));

        // When
        var result = await groupRunner.RunAsync("env", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedOnceExactly();

        await Verify(
            new
            {
                output,
                commandRunners,
            })
            .UseParameters(isWindows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_return_first_error_hit(bool isWindows)
    {
        // Given
        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(123),
            new(0),
            new(999),
        };

        var (_, groupRunner) = SetUpTest(commandRunners, isWindows);

        // When
        var result = await groupRunner.RunAsync("pack", null);

        // Then
        result.ShouldBe(123);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedTwiceExactly();

        await Verify(commandRunners).UseParameters(isWindows);
    }

    private static (StringWriter output, CommandGroupRunner groupRunner) SetUpTest(
        TestCommandRunner[] commandRunners,
        bool isWindows,
        Action<Dictionary<string, string?>>? scriptSetup = null)
    {
        var output = new StringWriter();
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = false,
        };
        var consoleWriter = new ConsoleWriter(output, consoleFormatProvider, verbose: true);

        var scripts = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            // clean
            { "clean", "echo clean" },

            // build
            { "prebuild", "echo prebuild" },
            { "build", "echo build" },

            // test
            { "test", "echo test" },
            { "posttest", "echo posttest" },

            // pack
            { "prepack", "echo pack" },
            { "pack", "echo pack" },
            { "postpack", "echo pack" },
        };

        scriptSetup?.Invoke(scripts);

        var environment = new TestEnvironment(
            "/test/path",
            isWindows);

        environment.SetEnvironmentVariable("value1", "value 1");
        environment.SetEnvironmentVariable("value2", "value 2");
        environment.SetEnvironmentVariable("value3", "value 3");

        var context = ProcessContext.Create(
            isWindows ? "cmd" : "sh",
            isWindows,
            "/test/path");

        var groupRunner = A.Fake<CommandGroupRunner>(
            o => o.WithArgumentsForConstructor(
                () => new CommandGroupRunner(
                    consoleWriter,
                    environment,
                    scripts,
                    context,
                    true,
                    default)));

        A.CallTo(() => groupRunner.BuildCommand()).ReturnsNextFromSequence(commandRunners);

        return (output, groupRunner);
    }

    private class TestCommandRunner : ICommandRunner
    {
        private readonly int _result;

        public TestCommandRunner(int result)
            => _result = result;

        public string? Name { get; private set; }

        public string? Cmd { get; private set; }

        public IReadOnlyList<string>? Args { get; private set; }

        public Task<int> RunAsync(string name, string cmd, string[]? args)
        {
            Name = name;
            Cmd = cmd;
            Args = args;

            return Task.FromResult(_result);
        }
    }
}

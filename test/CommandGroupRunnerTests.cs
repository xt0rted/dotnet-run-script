namespace RunScript;

using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Threading.Tasks;

[UsesVerify]
public class CommandGroupRunnerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_a_single_script(bool isWindows)
    {
        // Given
        var verifySettings = new VerifySettings();
        verifySettings.UseParameters(isWindows);

        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(1),
        };

        var groupRunner = SetUpTest(isWindows);
        A.CallTo(() => groupRunner.BuildCommand()).ReturnsNextFromSequence(commandRunners);

        // When
        var result = await groupRunner.RunAsync("clean", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedOnceExactly();

        await Verify(commandRunners, verifySettings);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_with_a_pre_script(bool isWindows)
    {
        // Given
        var verifySettings = new VerifySettings();
        verifySettings.UseParameters(isWindows);

        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(0),
            new(1),
        };

        var groupRunner = SetUpTest(isWindows);
        A.CallTo(() => groupRunner.BuildCommand()).ReturnsNextFromSequence(commandRunners);

        // When
        var result = await groupRunner.RunAsync("build", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedTwiceExactly();

        await Verify(commandRunners, verifySettings);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_with_a_post_script(bool isWindows)
    {
        // Given
        var verifySettings = new VerifySettings();
        verifySettings.UseParameters(isWindows);

        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(0),
            new(1),
        };

        var groupRunner = SetUpTest(isWindows);
        A.CallTo(() => groupRunner.BuildCommand()).ReturnsNextFromSequence(commandRunners);

        // When
        var result = await groupRunner.RunAsync("test", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappenedTwiceExactly();

        await Verify(commandRunners, verifySettings);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_run_with_a_pre_and_post_script(bool isWindows)
    {
        // Given
        var verifySettings = new VerifySettings();
        verifySettings.UseParameters(isWindows);

        TestCommandRunner[] commandRunners =
        {
            new(0),
            new(0),
            new(0),
            new(1),
        };

        var groupRunner = SetUpTest(isWindows);
        A.CallTo(() => groupRunner.BuildCommand()).ReturnsNextFromSequence(commandRunners);

        // When
        var result = await groupRunner.RunAsync("pack", null);

        // Then
        result.ShouldBe(0);

        A.CallTo(() => groupRunner.BuildCommand()).MustHaveHappened(3, Times.Exactly);

        await Verify(commandRunners, verifySettings);
    }

    private static CommandGroupRunner SetUpTest(bool isWindows)
    {
        var console = new TestConsole();
        var consoleFormatProvider = new ConsoleFormatInfo
        {
            SupportsAnsiCodes = false,
        };
        var consoleWriter = new ConsoleWriter(console, consoleFormatProvider, verbose: true);

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

        var environment = new TestEnvironment(
            "/test/path",
            isWindows);

        var context = ProcessContext.Create(
            isWindows ? "cmd" : "sh",
            isWindows,
            "/test/path");

        return A.Fake<CommandGroupRunner>(
            o => o.WithArgumentsForConstructor(
                () => new CommandGroupRunner(
                    consoleWriter,
                    environment,
                    scripts,
                    context,
                    default)));
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

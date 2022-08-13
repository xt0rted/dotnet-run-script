namespace RunScript;

using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

public static class RunScriptCommandTests
{
    [Trait("category", "unit")]
    public class FindScripts
    {
        private readonly Dictionary<string, string?> _projectScripts;

        public FindScripts()
        {
            _projectScripts = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                { "clean", "echo clean" },
                { "prebuild", "echo prebuild" },
                { "build", "echo build" },
                { "test", "echo test" },
                { "posttest", "echo posttest" },
                { "prepack", "echo pack" },
                { "pack", "echo pack" },
                { "postpack", "echo pack" },
            };
        }

        [Fact]
        public void Should_match_exact_script_names()
        {
            // Given
            var scripts = new[]
            {
                "build",
                "test",
                "magic",
            };

            // When
            var result = RunScriptCommand.FindScripts(
                _projectScripts,
                scripts);

            // Then
            result.ShouldBe(
                new List<ScriptResult>
                {
                    new("build", true),
                    new("test", true),
                    new("magic", false),
                });
        }

        [Fact]
        public void Should_match_wildcard_script_names()
        {
            // Given
            var scripts = new[]
            {
                "pre*",
                "magic*",
            };

            // When
            var result = RunScriptCommand.FindScripts(
                _projectScripts,
                scripts);

            // Then
            result.ShouldBe(
                new List<ScriptResult>
                {
                    new("prebuild", true),
                    new("prepack", true),
                    new("magic*", false),
                });
        }
    }

    [Trait("category", "unit")]
    [UsesVerify]
    public class RunResults
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(13)]
        public async Task Should_handle_single_result(int exitCode)
        {
            // Given
            var (console, writer) = SetUpTest();
            var results = new List<RunResult>
            {
                new("build", exitCode),
            };

            // When
            var result = RunScriptCommand.RunResults(
                writer,
                results);

            // Then
            result.ShouldBe(exitCode);

            await Verify(console).UseParameters(exitCode);
        }

        [Fact]
        public async Task Should_handle_multiple_success_results()
        {
            // Given
            var (console, writer) = SetUpTest();
            var results = new List<RunResult>
            {
                new("build", 0),
                new("test", 0),
            };

            // When
            var result = RunScriptCommand.RunResults(
                writer,
                results);

            // Then
            result.ShouldBe(0);

            await Verify(console);
        }

        [Fact]
        public async Task Should_handle_multiple_error_results()
        {
            // Given
            var (console, writer) = SetUpTest();
            var results = new List<RunResult>
            {
                new("build", 13),
                new("test", 99),
            };

            // When
            var result = RunScriptCommand.RunResults(
                writer,
                results);

            // Then
            result.ShouldBe(1);

            await Verify(console);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(13)]
        public async Task Should_handle_any_error_result(int exitCode)
        {
            // Given
            var (console, writer) = SetUpTest();
            var results = new List<RunResult>
            {
                new("build", 0),
                new("test", exitCode),
            };

            // When
            var result = RunScriptCommand.RunResults(
                writer,
                results);

            // Then
            result.ShouldBe(1);

            await Verify(console).UseParameters(exitCode);
        }

        private static (TestConsole console, IConsoleWriter writer) SetUpTest()
        {
            var console = new TestConsole();
            var consoleWriter = new ConsoleWriter(
                console,
                new ConsoleFormatInfo
                {
                    SupportsAnsiCodes = false,
                },
                verbose: true);

            return (console, consoleWriter);
        }
    }
}

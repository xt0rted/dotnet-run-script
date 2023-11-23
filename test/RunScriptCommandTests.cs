namespace RunScript;

using System.Collections.Generic;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

public static class RunScriptCommandTests
{
    [Trait("category", "unit")]
    public class FindScripts
    {
        private readonly ScriptCollection _projectScripts;

        public FindScripts()
        {
            _projectScripts = new ScriptCollection
            {
                { "clean", "echo clean" },
                { "prebuild", "echo prebuild" },
                { "build", "echo build" },
                { "test", "echo test" },
                { "posttest", "echo posttest" },
                { "prepack", "echo pack" },
                { "pack", "echo pack" },
                { "postpack", "echo pack" },
                { "foo", "foo" },
                { "foo:foo", "foo:foo" },
                { "foo:bar", "foo:bar" },
                { "foo:baz", "foo:baz" },
                { "foo:foo:foo", "foo:foo:foo" },
                { "foo:foo:bar", "foo:foo:bar" },
                { "foo:foo:baz", "foo:foo:baz" },
                { "foo:bar:foo", "foo:bar:foo" },
                { "foo:bar:bar", "foo:bar:bar" },
                { "foo:bar:baz", "foo:bar:baz" },
                { "foo:baz:foo", "foo:baz:foo" },
                { "foo:baz:bar", "foo:baz:bar" },
                { "foo:baz:baz", "foo:baz:baz" },
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
                "foo*",
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
                    new("foo", true),
                    new("prebuild", true),
                    new("prepack", true),
                    new("magic*", false),
                });
        }

        [Fact]
        public void Should_match_only_1_segment()
        {
            // Given
            var scripts = new[]
            {
                "foo:*"
            };

            // When
            var result = RunScriptCommand.FindScripts(
                _projectScripts,
                scripts);

            // Then
            result.ShouldBe(
                new List<ScriptResult>
                {
                    new("foo:foo", true),
                    new("foo:bar", true),
                    new("foo:baz", true),
                });
        }

        [Fact]
        public void Should_match_only_1_trailing_segment()
        {
            // Given
            var scripts = new[]
            {
                "foo:bar:*"
            };

            // When
            var result = RunScriptCommand.FindScripts(
                _projectScripts,
                scripts);

            // Then
            result.ShouldBe(
                new List<ScriptResult>
                {
                    new("foo:bar:foo", true),
                    new("foo:bar:bar", true),
                    new("foo:bar:baz", true),
                });
        }

        [Fact]
        public void Should_match_multiple_segments()
        {
            // Given
            var scripts = new[]
            {
                "foo:**"
            };

            // When
            var result = RunScriptCommand.FindScripts(
                _projectScripts,
                scripts);

            // Then
            result.ShouldBe(
                new List<ScriptResult>
                {
                    new("foo:foo", true),
                    new("foo:bar", true),
                    new("foo:baz", true),
                    new("foo:foo:foo", true),
                    new("foo:foo:bar", true),
                    new("foo:foo:baz", true),
                    new("foo:bar:foo", true),
                    new("foo:bar:bar", true),
                    new("foo:bar:baz", true),
                    new("foo:baz:foo", true),
                    new("foo:baz:bar", true),
                    new("foo:baz:baz", true),
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

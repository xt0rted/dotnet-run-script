# <img src="assets/icon.svg" align="left" height="45"> dotnet-run-script

[![CI build status](https://github.com/xt0rted/dotnet-run-script/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/xt0rted/dotnet-run-script/actions/workflows/ci.yml)
[![NuGet Package](https://img.shields.io/nuget/v/run-script?logo=nuget)](https://www.nuget.org/packages/run-script)
[![GitHub Package Registry](https://img.shields.io/badge/github-package_registry-yellow?logo=nuget)](https://nuget.pkg.github.com/xt0rted/index.json)
[![Project license](https://img.shields.io/github/license/xt0rted/dotnet-run-script)](LICENSE)

A `dotnet` tool to run arbitrary commands from a project's "scripts" object.
If you've used `npm` this is the equivalent of `npm run` with almost identical functionality and options.
It is compatible with .NET Core 3.1 and newer.

See [the about page](docs/README.md) for more information on how this tool came to be and why it exists at all.

## Installation

This tool is meant to be used as a dotnet local tool.
To install it run the following:

```console
dotnet new tool-manifest
dotnet tool install run-script
```

> [!WARNING]
> Installing this tool globally is not recommended.
> PowerShell defines the alias `r` for the `Invoke-History` command which prevents this from being called.
> You'll also run into issues calling this from your scripts since global tools don't use the `dotnet` prefix.

## Keeping current

Tools like [Dependabot](https://github.com/github/feedback/discussions/13825) and [Renovate](https://github.com/marketplace/renovate) don't currently support updating dotnet local tools.
One way to automate this is to use a [GitHub Actions workflow](https://github.com/xt0rted/dotnet-tool-update-test) to check for updates and create PRs when new versions are available, which is what this repo does.

## Options

Name | Description
-- | --
`--if-present` | Don't exit with an error code if the script isn't found
`--script-shell` | The shell to use when running scripts (cmd, pwsh, sh, etc.)
`-v`, `--verbose` | Enable verbose output
`--version` | Show version information
`--help` | Show help and usage information

Arguments passed after the double dash are passed through to the executing script.

```console
dotnet r build --verbose -- --configuration Release
```

### Color output

This tool supports the `DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION` environment variable.
Setting this to `1` or `true` will force color output on all platforms.
Due to a limitation of the `Console` apis this will not work on Windows when output is redirected in environments such as GitHub Actions.

There is also support for the `NO_COLOR` environment variable.
Setting this to any value will disable color output.

### GitHub Actions

On GitHub Actions you need to set the environment values `DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION` and `TERM`.
`TERM` should be `xterm` or `xterm-256color`.

## Configuration

In your project's `global.json` add a `scripts` object:

```jsonc
{
  "sdk": {
    "version": "8.0.203",
    "rollForward": "latestPatch"
  },
  "scriptShell": "pwsh", // Optional
  "scripts": {
    "clean": "dotnet r clean:*",
    "clean:artifacts": "dotnet rimraf artifacts", // dotnet tool install rimraf
    "clean:bin": "dotnet rimraf **/bin **/obj",
    "build": "dotnet build --configuration Release",
    "test": "dotnet test --configuration Release",
    "ci": "dotnet r build && dotnet r test",
  }
}
```

> [!NOTE]
> The shell used depends on the OS.
> On Windows `CMD` is used, on Linux, macOS, and WSL `sh` is used.
> This can be overridden by setting the `scriptShell` property or by passing the `--script-shell` option with the name of the shell to use.

The `env` command is a special built-in command that lists all available environment variables.
You can override this with your own command if you wish.

## Usage

Use `dotnet r [<scripts>...] [options]` to run the scripts.
Anything you can run from the command line can be used in a script.
You can also call other scripts to chain them together such as a `ci` script that calls the `build`, `test`, and `package` scripts.

To help keep your configuration easy to read and maintain `pre` and `post` scripts are supported.
These are run before and after the main script.

This is an example of a `pre` script that clears the build artifacts folder, and a `post` script that writes to the console saying the command completed.

```json
{
  "scripts": {
    "prepackage": "del /Q ./artifacts",
    "package": "dotnet pack --configuration Release --no-build --output ./artifacts",
    "postpackage": "echo \"Packaging complete\""
  }
}
```

### Multiple script execution

Multiple scripts can be called at the same time like so:

```console
dotnet r build test
```

This will run the `build` script and if it returns a `0` exit code it will then run the `test` script.
The `--if-present` option can be used to skip scripts that don't exist.

```json
{
  "scripts": {
    "build": "dotnet build",
    "test:unit": "dotnet test",
    "package": "dotnet pack"
  }
}
```

```console
dotnet r build test:unit test:integration package --if-present
```

Arguments passed after the double dash are passed through to each executing script.
In this example both the `--configuration` and `--framework` options will be passed to each of the four scripts when running them.

```console
dotnet r build test:unit test:integration package -- --configuration Release --framework net8.0
```

### Globbing or wildcard support

Multiple scripts can be run at the same time using globbing.
This means `dotnet r test:*` will match `test:unit` and `test:integration` and run them in series in the order they're listed in the `global.json` file.

Globbing is handled by the [DotNet.Glob](https://github.com/dazinator/DotNet.Glob) library and currently supports all of its patterns and wildcards.

### Working directory

The working directory is set to the root of the project where the `global.json` is located.
If you need to get the folder the command was executed from you can do so using the `INIT_CWD` environment variable.

## Common build environments

When using this tool on a build server, such as GitHub Actions, you might want to use a generic workflow that calls a common set of scripts such as `build`, `test`, and `package`.
These might not be defined in all of your projects and if a script that doesn't exist is called an error is returned.
To work around this you can call them with the `--if-present` flag which will return a `0` exit code for not found scripts.

Example shared GitHub Actions workflow:

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      # Always runs
      - name: Run build
        run: dotnet r build

      # Only runs if `test` script is present
      - name: Run test
        run: dotnet r test --if-present

      # Only runs if `package` script is present
      - name: Run package
        run: dotnet r package --if-present
```

# Changelog

## Unreleased

## [0.4.0](https://github.com/xt0rted/dotnet-run-script/compare/v0.3.0...v0.4.0) - 2022-08-12

> **Note**
> This version drops support for .NET 5 which is no longer supported, but it will continue to work with .NET 5 SDKs.

### Added

- Ability to run multiple scripts in a single call (e.g. `dotnet r build test pack`) ([#10](https://github.com/xt0rted/dotnet-run-script/pull/10))
- Support for globbing in script names (e.g `dotnet r test:*` will match `test:unit` and `test:integration`) ([#79](https://github.com/xt0rted/dotnet-run-script/pull/79
))

### Updated

- Bumped `System.CommandLine` from 2.0.0-beta3.22114.1 to 2.0.0-beta4.22272.1
- Bumped `System.CommandLine.Rendering` from 0.4.0-alpha.22114.1 to 0.4.0-alpha.22272.1

## [0.3.0](https://github.com/xt0rted/dotnet-run-script/compare/v0.2.0...v0.3.0) - 2022-04-24

### Fixed

- Don't escape the script passed to `cmd.exe`, just the double dash arguments

## [0.2.0](https://github.com/xt0rted/dotnet-run-script/compare/v0.1.0...v0.2.0) - 2022-04-23

> **Note**
> This version broke conditional script execution (`cmd1 && cmd2`) in `cmd.exe`

### Added

- Force color output with the `DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION` environment variable.
  - Note: this tool with output color on all platforms including when output is redirected, but the dotnet cli only supports this on Unix platforms currently. This means script results might not be colored in places like GitHub Actions build logs when using the Windows VMs.
- Added `-v` alias to enable verbose output.

### Fixed

- Escape arguments for non-cmd shells
- Quote additional arguments passed after `--`
- Escape scripts with `^` passed to `cmd.exe`

### Updated

- Switched from [actions/setup-dotnet](https://github.com/actions/setup-dotnet) to [xt0rted/setup-dotnet](https://github.com/xt0rted/setup-dotnet)

## [0.1.0](https://github.com/xt0rted/dotnet-run-script/releases/tag/v0.1.0) - 2022-03-26

- Run scripts with `dotnet r ...`
- Uses `cmd` on Windows and `sh` on Linux
- Support for `pre` and `post` scripts
- Support for setting a custom shell such as `pwsh`
  - Set via the `scriptShell` global.json setting
  - Set via the `--script-shell` parameter
- Built-in `env` command to list available environment variables
- Flow parameters after `--` to the running script
- Skip commands that aren't found with the `--if-present` parameter

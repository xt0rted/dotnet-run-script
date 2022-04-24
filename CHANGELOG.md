# Changelog

## Unreleased

- fixed: Don't escape the script passed to `cmd.exe`, just the double dash arguments

## [0.2.0](https://github.com/xt0rted/dotnet-run-script/compare/v0.1.0...v0.2.0) - 2022-04-23

> ℹ️ This version broke conditional script execution (`cmd1 && cmd2`) in `cmd.exe`

- added: Force color output with the `DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION` environment variable.
  - Note: this tool with output color on all platforms including when output is redirected, but the dotnet cli only supports this on Unix platforms currently. This means script results might not be colored in places like GitHub Actions build logs when using the Windows VMs.
- added: Added `-v` alias to enable verbose output.
- fixed: Escape arguments for non-cmd shells
- fixed: Quote additional arguments passed after `--`
- fixed: Escape scripts with `^` passed to `cmd.exe`
- updated: Switched from [actions/setup-dotnet](https://github.com/actions/setup-dotnet) to [xt0rted/setup-dotnet](https://github.com/xt0rted/setup-dotnet)

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

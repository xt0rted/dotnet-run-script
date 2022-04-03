# Changelog

## Unreleased

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

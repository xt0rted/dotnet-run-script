# About

After many years using `npm` it always bothered me that there wasn't an easy way to define `build`, `test`, and `package` scripts for my .NET projects.
I'd usually end up using PowerShell scripts in a `/scripts` folder or a tool like [Cake](https://cakebuild.net/), both of which can be cumbersome to discover and use, especially for newcomers to a project who most likely aren't familiar with them.

When .NET Core introduced the `dotnet` cli it became easier to do these things, but you still needed to pass additional arguments to build in release mode or put test results in a common folder to upload to Codecov or Coveralls.

A great example of this is collecting code coverage of unit tests with [Coverlet](https://github.com/coverlet-coverage/coverlet).

```shell
> dotnet test --configuration Debug --verbosity minimal --no-build --collect:"XPlat Code Coverage" --results-directory "./.coverage"
```

Now this can be configured in your `global.json` like so:

```json
{
  "scripts": {
    "test": "dotnet test --configuration Debug --verbosity minimal --no-build",
    "test:coverage": "dotnet r test -- --collect:\\\"XPlat Code Coverage\\\" --results-directory \"./.codecoverage\""
  }
}
```

Now a basic test run can be done by calling:

```shell
> dotnet r test
```

Or code coverage can be collected by calling:

```shell
> dotnet r test:coverage
```

### What this isn't

This tool is not a replacement for a more robust build tool like [Cake](https://cakebuild.net/) or [Fake](https://fake.build/).
Instead it's meant to be used as a way to more easily call them by defining a common set of commands like `clean`, `build`, `test`, `package`, or `publish` that can be added to all of your projects without the user knowing, or caring, how they work or what they do.
By defining them in the project's `global.json` they're easy to find, and running `dotnet r` or `dotnet r --help` will give a list of available scripts so you don't have to go looking for them.

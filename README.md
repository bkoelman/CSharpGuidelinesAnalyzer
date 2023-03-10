# CSharp Guidelines Analyzer

[![Build status](https://ci.appveyor.com/api/projects/status/q37dldfggtcwf6u4/branch/master?svg=true)](https://ci.appveyor.com/project/bkoelman/csharpguidelinesanalyzer/branch/master)
[![codecov](https://codecov.io/gh/bkoelman/CSharpGuidelinesAnalyzer/branch/master/graph/badge.svg)](https://codecov.io/gh/bkoelman/CSharpGuidelinesAnalyzer)

This Visual Studio analyzer supports you in making your code comply with the C# coding guidelines at [CSharpGuidelines](https://github.com/dennisdoomen/CSharpGuidelines).

Note that many guidelines are already covered by [Resharper](https://www.jetbrains.com/resharper/), for which [a layer file is provided](/docs/Resharper%20Settings.md).
See [Overview](/docs/Overview.md) for the list of supported rules.

![Analyzer in action](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/gh-pages/images/analyzer-in-action.png)

## Get started

The latest version requires [Visual Studio 2017 with Update 5](https://www.visualstudio.com/) or higher. To get instant feedback on all files in your solution, activate [Full Solution Analysis](/docs/Full%20Solution%20Analysis.md).

* From the NuGet package manager console:

  `Install-Package CSharpGuidelinesAnalyzer`

  or, if you are using Visual Studio 2017 with Update 3:

  `Install-Package CSharpGuidelinesAnalyzer -version 2.0.0`

  or, if you are using Visual Studio 2015 with Update 2 or higher:

  `Install-Package CSharpGuidelinesAnalyzer -version 1.0.1`

* Rebuild your solution

* Optional: [Reference CSharpGuidelines.Layer.DotSettings in your existing Resharper preferences](/docs/Resharper%20Settings.md)

## Rule configuration
The behavior of a few rules can optionally be customized using a configuration file. See [documentation](docs/Configuration.md) for details.

## Suppressing rules
Rule warnings can be suppressed at various scopes, ranging from per line to at the project or solution level.

* With `#pragma` lines, for example:
```csharp
#pragma warning disable AV1532 // Loop statement contains nested loop
    foreach (string item in itemArray)
#pragma warning restore AV1532 // Loop statement contains nested loop
```
On the location of a warning, press **Ctrl+.** or **Alt+Enter** and select **Suppress**, **in Source**.

* In `GlobalSuppressions.cs`, for example:
```csharp
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1532:Loop statement contains nested loop", Justification = "<Pending>", Scope = "member", Target = "~M:CSharpGuidelinesDemo.Demo.RunDemo(System.String[][],System.Boolean,System.String)~System.Collections.Generic.List{System.String}")]
```
On the location of a warning, press **Ctrl+.** or **Alt+Enter** and select **Suppress**, **in Suppression File**.
Note that you can broaden the suppression scope by removing the `Target` and/or `Scope` attributes:

```csharp
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1532:Loop statement contains nested loop", Justification = "<Pending>")]
```

* In an .editorconfig file, which contains [rule severities](https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019#set-rule-severity-in-an-editorconfig-file):

```ini
root = true

[*.cs]
dotnet_diagnostic.av1115.severity = error
dotnet_diagnostic.av1130.severity = suggestion
```

* In a custom .ruleset file, which contains Code Analysis settings:

Right-click your project, select **Properties**, tab **Code Analysis**. Click **Open**, expand **CSharpGuidelinesAnalyzers** and uncheck the rules you want to disable. When you save changes, a .ruleset file is added to your project.

Alternatively, navigate to your project in **Solution Explorer** and expand **References**, **Analyzers**, **CSharpGuidelinesAnalyzer**. Then right-click on one of the rules and select **Set Rule Set Severity**. These changes are stored in a .ruleset file in your project.

To apply the custom ruleset to the entire solution, move the .ruleset file next to your .sln file and browse to it on the CodeAnalysis tab for each project.

## Performance

If you run these analyzers on a large codebase and are concerned about performance, consider disabling AV1568 and AV1739. These two are by far the most resource-intensive.

## Contribute!

The analyzers in this project benefit a lot from testing on various codebases. Some of the best ways to contribute are to try things out, file bugs, and join in design conversations.

## Trying out the latest build

After each commit, a new prerelease NuGet package is automatically published to AppVeyor at https://ci.appveyor.com/project/bkoelman/csharpguidelinesanalyzer/branch/master/artifacts. To try it out, follow the next steps:

* In Visual Studio: **Tools**, **NuGet Package Manager**, **Package Manager Settings**, **Package Sources**
    * Click **+**
    * Name: **AppVeyor CSharpGuidelinesAnalyzer**, Source: **https://ci.appveyor.com/nuget/csharpguidelinesanalyzer**
    * Click **Update**, **Ok**
* Open the NuGet package manager console (**Tools**, **NuGet Package Manager**, **Package Manager Console**)
    * Select **AppVeyor CSharpGuidelinesAnalyzer** as package source
    * Run command: `Install-Package CSharpGuidelinesAnalyzer.NuGetBugRequiresNewId -pre`

## Building from source

Clone the repository and open `CSharpGuidelinesAnalyzer.sln` in Visual Studio.
You can now build and run the tests.

To debug an analyzer, set a breakpoint and press F5. This launches a second (experimental) Visual Studio instance with the debugger attached. In the experimental instance, open a project and observe your breakpoint get hit.

Note: When using Visual Studio 2022 or higher, breakpoints don't get hit unless you change the following setting **in the experimental instance**:

* Tools > Options > Text Editor > C# > Advanced > Uncheck 'Run code analysis in separate process'

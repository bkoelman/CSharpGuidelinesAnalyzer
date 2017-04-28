# CSharp Guidelines Analyzer

[![Build status](https://ci.appveyor.com/api/projects/status/q37dldfggtcwf6u4/branch/master?svg=true)](https://ci.appveyor.com/project/bkoelman/csharpguidelinesanalyzer/branch/master)
[![codecov](https://codecov.io/gh/bkoelman/CSharpGuidelinesAnalyzer/branch/master/graph/badge.svg)](https://codecov.io/gh/bkoelman/CSharpGuidelinesAnalyzer)
[![csharpguidelinesanalyzer MyGet Build Status](https://www.myget.org/BuildSource/Badge/csharpguidelinesanalyzer?identifier=757dfdd3-26d5-4842-abac-4cdf820e3f6d)](https://www.myget.org/)

This Visual Studio analyzer supports you in making your code comply with the C# coding guidelines at [CSharpGuidelines](https://github.com/dennisdoomen/CSharpGuidelines). 

Note that many guidelines are already covered by [Resharper](https://www.jetbrains.com/resharper/), for which [a layer file is provided](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Resharper%20Settings.md).
See [Overview](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Overview.md) for the list of supported rules.

![Analyzer in action](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/gh-pages/images/analyzer-in-action.png)

## Get started

* At the minimum, you need [Visual Studio 2015 with Update 2](https://www.visualstudio.com/). If you are using Update 3 (or Visual Studio 2017), it is recommended to [activate Full Solution Analysis](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Full%20Solution%20Analysis.md).

* From the NuGet package manager console:

  `Install-Package CSharpGuidelinesAnalyzer`

* Rebuild your solution

* Recommended: [Reference CSharpGuidelines.Layer.DotSettings in your existing Resharper preferences](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Resharper%20Settings.md)

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

**Note:** Due to a [bug in Resharper](https://youtrack.jetbrains.com/issue/RSRP-461893) you may not see the **Suppress** context menu item. To make the option accessible, temporarily suspend Resharper.

* In a custom .ruleset file, which contains Code Analysis settings:

Right-click your project, select **Properties**, tab **Code Analysis**. Click **Open**, expand **CSharpGuidelinesAnalyzers** and uncheck the rules you want to disable. When you save changes, a .ruleset file is added to your project.

To apply the custom ruleset to the entire solution, move the .ruleset file next to your .sln file and browse to it on the CodeAnalysis tab for each project.

## Contribute!

The analyzers in this project benefit a lot from testing on various codebases. Some of the best ways to contribute are to try things out, file bugs, and join in design conversations.

## Trying out the latest build

After each commit, a new prerelease NuGet package is automatically published to [MyGet](http://www.myget.org). To try it out, follow the next steps:

* In Visual Studio: **Tools**, **Options**, **NuGet Package Manager**, **Package Sources**
    * Click **+**
    * Name: **MyGet**, Source: **https://www.myget.org/F/csharpguidelinesanalyzer**
    * Click **Update**, **Ok**
* Open the NuGet package manager console  (**Tools**, **NuGet Package Manager**, **Package Manager Console**)
    * Select **MyGet** as package source
    * Run command: `Install-Package CSharpGuidelinesAnalyzer -pre`

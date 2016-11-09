# CSharp Guidelines Analyzer

[![Build status](https://ci.appveyor.com/api/projects/status/q37dldfggtcwf6u4/branch/master?svg=true)](https://ci.appveyor.com/project/bkoelman/csharpguidelinesanalyzer/branch/master)
[![csharpguidelinesanalyzer MyGet Build Status](https://www.myget.org/BuildSource/Badge/csharpguidelinesanalyzer?identifier=757dfdd3-26d5-4842-abac-4cdf820e3f6d)](https://www.myget.org/)

This Visual Studio analyzer supports you in making your code comply with the C# coding guidelines at [CSharpGuidelines](https://github.com/dennisdoomen/CSharpGuidelines). Note that many guidelines are already covered by [Resharper](https://www.jetbrains.com/resharper/), which are not implemented here.

![Analyzer in action](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/gh-pages/images/analyzer-in-action.png)

## Project status

All analyzers have been implemented, but testing and optimization work is still in progress. See [Overview](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Overview.md) for the list of supported rules and their status.

## Get started

* At the minimum, you need [Visual Studio](https://www.visualstudio.com/) 2015. About 50% of the rule analyzers [require Update 2 or later](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md). Without Update 2, those will remain silent. If you are using Update 3, you need to activate [Full Solution Analysis](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Full%20Solution%20Analysis.md) for all analyzers to work.

* From the NuGet package manager console:

  `Install-Package CSharpGuidelinesAnalyzer -Pre`

* Rebuild your solution

## Contribute!

This analyzer pack still needs a lot of testing on various codebases. Some of the best ways to contribute are to try things out, file bugs, and join in design conversations.

## Trying out the latest build

After each commit, a new prerelease NuGet package is automatically published to [MyGet](http://www.myget.org). To try it out, follow the next steps:

* In Visual Studio: **Tools**, **Options**, **NuGet Package Manager**, **Package Sources**
    * Click **+**
    * Name: **MyGet**, Source: **https://www.myget.org/F/csharpguidelinesanalyzer**
    * Click **Update**, **Ok**
* Open the NuGet package manager console  (**Tools**, **NuGet Package Manager**, **Package Manager Console**)
    * Select **MyGet** as package source
    * Run command: `Install-Package CSharpGuidelinesAnalyzer -pre`

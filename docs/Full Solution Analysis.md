# Full Solution Analysis for Managed Code

Visual Studio 2015 Update 2 introduced [Full Solution Analysis](https://msdn.microsoft.com/en-us/library/mt709421.aspx), which was enabled by default for C# projects. 

To improve performance of Visual Studio, in Update 3 the default was changed to **disabled** for C# projects.

When disabled, live analysis only runs on the currently active file. Analysis of other files and project-wide analysis does not run. You need to rebuild your solution for all diagnostics to show up.

If you are using Visual Studio 2015 Update 3 or Visual Studio 2017 and want live feedback, it is recommended to enable Full Solution Analysis.

Note there is [some ongoing discussion](https://github.com/dotnet/roslyn/issues/11750) about this.

# Full Solution Analysis for Managed Code

Visual Studio 2015 Update 3 introduced [Full Solution Analysis](https://msdn.microsoft.com/en-us/library/mt709421.aspx), which is disabled by default for C# projects.

This option was added to improve performance of Visual Studio. By default, live analysis only runs on the currently active file. Analysis on other files and project-wide analysis does not run. You need to rebuild your solution for all diagnostics to show up.

If you want live feedback, it is recommended to enable Full Solution Analysis.

Note there is [some ongoing discussion](https://github.com/dotnet/roslyn/issues/11750) about this feature.

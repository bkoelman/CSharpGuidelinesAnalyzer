# Full Solution Analysis for Managed Code

If you are using Visual Studio 2015 Update 3 or later, some analyzers will not run by default. This is because [Full Solution Analysis](https://msdn.microsoft.com/en-us/library/mt709421.aspx) is disabled by default for C# projects, combined with [this bug](https://github.com/dotnet/roslyn/issues/11750).

To enable these analyzers, enable Full Solution Analysis by following [these steps](https://msdn.microsoft.com/en-us/library/mt709421.aspx).

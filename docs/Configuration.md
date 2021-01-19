# Rule configuration

Rule behavior can be configured in [.editorconfig](https://editorconfig.org/) files, for example:

```ini
root = true

[*.cs]
dotnet_diagnostic.av1500.max_statement_count = 12
dotnet_diagnostic.av1561.max_parameter_count = 5
dotnet_diagnostic.av1561.max_constructor_parameter_count = 8
```

Editorconfig settings are inherited from parent directories (unless `root = true`), which enables you to vary rule configuration per directory.

> Note: A [bug](https://youtrack.jetbrains.com/issue/RIDER-53508) in JetBrains Rider prevents reading these settings from .editorconfig. If you're using Rider, use the legacy format instead (see below).

Aside from rule-specific settings, you can set [severities](https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019#set-rule-severity-in-an-editorconfig-file) for all rules in this file too:

```ini
root = true

[*.cs]
dotnet_diagnostic.av1115.severity = error
dotnet_diagnostic.av1130.severity = suggestion
```

# Legacy configuration support

Note: The method described here still exists for compatibility with earlier versions (editorconfig takes precedence), but will likely be removed in a future version.

The behavior of rules can be customized by adding a file named `CSharpGuidelinesAnalyzer.config` to your C# project with the following structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<cSharpGuidelinesAnalyzerSettings>
  <setting rule="AV1500" name="MaxStatementCount" value="12" />
  <setting rule="AV1561" name="MaxParameterCount" value="5" />
  <setting rule="AV1561" name="MaxConstructorParameterCount" value="8" />
</cSharpGuidelinesAnalyzerSettings>
```

The next step is to change the Build Action (Properties window) of this file to **AdditionalFiles** (or **C# analyzer additional file**, depending on project type). This should result in the following line added to your project file:

```xml
<AdditionalFiles Include="CSharpGuidelinesAnalyzer.config" />
```

For a complete list of the available rules and their configuration settings, see [Overview](/docs/Overview.md).

## Multiple projects

If your solution consists of multiple projects, you can share the configuration by moving `CSharpGuidelinesAnalyzer.config` to the solution folder and updating your project files to use a relative link:
```xml
<AdditionalFiles Include="..\CSharpGuidelinesAnalyzer.config">
  <Link>CSharpGuidelinesAnalyzer.config</Link>
</AdditionalFiles>
```

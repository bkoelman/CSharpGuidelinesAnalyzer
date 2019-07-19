# Rule configuration

The behavior of rules can be customized by adding a file named `CSharpGuidelinesAnalyzer.config` to your C# project with the following structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<cSharpGuidelinesAnalyzerSettings>
  <setting rule="AV1500" name="MaxStatementCount" value="12" />
  <setting rule="AV1561" name="MaxParameterCount" value="5" />
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

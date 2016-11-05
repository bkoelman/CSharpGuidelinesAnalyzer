# IOperation support

About 50% of the rule analyzers depend on the `IOperation` API, which was introduced in Visual Studio 2015 Update 2. The NuGet installer automatically activates this API in your project.

If, for some reason, you want to activate this API yourself, add the  `<Features>IOperation</Features>` property to the first property group in your .csproj file.

Example:
```
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{4808BC4A-E87F-431E-BF5E-21CBEC48B97A}</ProjectGuid>
    <OutputType>Exe</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>CSharpGuidelinesDemo</RootNamespace>
    <AssemblyName>CSharpGuidelinesDemo</AssemblyName>
    <TargetFrameworkVersion>v4.5.2</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
    <Features>IOperation</Features>
  </PropertyGroup>
```

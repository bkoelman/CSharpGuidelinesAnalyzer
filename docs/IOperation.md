# IOperation support

Some analyzers depend on the `IOperation` API, which was introduced in Visual Studio 2015 Update 2. This API is enabled by default in the next Visual Studio version, but users of Visual Studio 2015 will need to modify their project files.

To enable these analyzers to run, add the  `<Features>IOperation</Features>` property to the first property group in your .csproj file.

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

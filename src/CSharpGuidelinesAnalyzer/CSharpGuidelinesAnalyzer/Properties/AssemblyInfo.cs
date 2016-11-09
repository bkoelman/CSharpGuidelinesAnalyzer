using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("CSharpGuidelinesAnalyzer")]
[assembly:
    AssemblyDescription("Reports diagnostics for C# coding guidelines that are not already covered by Resharper.")]

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("open source")]
[assembly: AssemblyProduct("CSharpGuidelinesAnalyzer")]
[assembly: AssemblyCopyright("Apache License, Version 2.0")]
[assembly: AssemblyTrademark("CSharpGuidelinesAnalyzer")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("0.2.0")]
[assembly: AssemblyInformationalVersion("0.2.0")]
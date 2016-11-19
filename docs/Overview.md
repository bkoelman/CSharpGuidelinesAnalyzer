# Implementation overview
The list below describes per rule what its analyzer reports on. Checked rules are implemented.

## Category: Class Design

### [AV1000](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1000_ClassDesignGuidelines.md#av1000): A class or interface should have a single purpose (HIGH)
- [x] This analyzer reports types that have the word "And" in their name.

### [AV1008](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1000_ClassDesignGuidelines.md#av1008): Avoid static classes (LOW)
- [x] This analyzer reports static classes with a name that does not end in "Extensions" and public/internal non-extension methods.

### [AV1010](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1000_ClassDesignGuidelines.md#av1010): Don't hide inherited members with the new keyword (HIGH)
- [x] This analyzer reports members that have the `new` modifier in their signature.

## Category: Member Design

### [AV1115](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1100_MemberDesignGuidelines.md#av1115): A method or property should do only one thing (HIGH)
- [x] This analyzer reports members that have the word "And" in their name.

### [AV1130](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1100_MemberDesignGuidelines.md#av1130): Return an IEnumerable<T> or ICollection<T> instead of a concrete collection class (MEDIUM)
- [x] This analyzer reports members whose signature returns a `class` or `struct` that implements `IEnumerable`.

## Category: Miscellaneous Design

### [AV1225](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1200_MiscellaneousDesignGuidelines.md#av1225): Use a protected virtual method to raise each event (MEDIUM)
- [x] This analyzer reports methods that invoke an event, which are non-protected or non-virtual or their name does not end with the word "On" followed by the event name.

### [AV1235](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1200_MiscellaneousDesignGuidelines.md#av1235): Don't pass null as the sender argument when raising an event (HIGH)
- [x] This analyzer reports when a non-static event is invoked with `null` for the "sender" parameter, or when an event is invoked with `null` for the "args" parameter.

### [AV1250](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1200_MiscellaneousDesignGuidelines.md#av1250): Evaluate the result of a LINQ expression before returning it (HIGH)
- [x] This analyzer reports when the result of a LINQ method that uses deferred execution is returned.

## Category: Maintainability

### [AV1500](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1500): Methods should not exceed 7 statements (HIGH)
- [x] This analyzer reports methods that consist of more than seven statements.

### [AV1502](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1502): Avoid conditions with double negatives (MEDIUM)
- [x] This analyzer reports usages of the logical not operator with an argument that contains the word "No" or "Not" in it.

### [AV1505](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1505): Name assemblies after their contained namespace (LOW)
- [x] This analyzer reports namespaces and types that do not match with the assembly name, unless the assembly name ends with ".Core".

### [AV1507](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1507): Limit the contents of a source code file to one type (LOW)
- [x] This analyzer reports when a file contains multiple non-nested types.

### [AV1522](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1522): Assign each variable in a separate statement (HIGH)
- [x] This analyzer reports when multiple properties, fields, parameters or variables are assigned in a single statement.

### [AV1525](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1525): Don't make explicit comparisons to true or false (HIGH)
- [x] This analyzer reports boolean comparisons with `true` or `false`.

### [AV1530](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1530): Don't change a loop variable inside a for loop (MEDIUM)
- [x] This analyzer reports when loop variables are assigned in the loop body.

### [AV1532](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1532): Avoid nested loops (MEDIUM)
- [x] This analyzer reports when `for`, `foreach`, `while` or `do-while` loops are nested.

### [AV1535](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1535): Always add a block after keywords such as if, else, while, for, foreach and case (MEDIUM)
- [x] This analyzer reports when a case statement does not have a block. The other scenarios are already covered by Resharper.

### [AV1537](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1537): Finish every if-else-if statement with an else-part (MEDIUM)
- [x] This analyzer reports `if-else-if` statements that do not end with an unconditional `else` clause.

### [AV1551](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1551): Call the more overloaded method from other overloads (MEDIUM)
- [x] This analyzer reports when the method overload with the most parameters is not declared virtual. For the other overloads, it reports if they do not delegate to another overload or when their parameter order is different.

### [AV1555](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1555): Avoid using named arguments (HIGH)
- [x] This analyzer reports invocations with named arguments that are not of type `bool` or `bool?`.

### [AV1561](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1561): Don't allow methods and constructors with more than three parameters (HIGH)
- [x] This analyzer reports methods, indexers and delegates that declare more than three parameters.

### [AV1564](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1564): Avoid methods that take a bool flag (HIGH)
- [x] This analyzer reports methods that declare parameters of type `bool` or `bool?`.

### [AV1568](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md#av1568): Don't use parameters as temporary variables (LOW)
- [x] This analyzer reports parameters that are written to and are not declared as `ref` or `out`.

## Category: Naming

### [AV1704](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1704): Don't include numbers in variables, parameters and type members (LOW)
- [x] This analyzer reports when a digit occurs in a type, member, parameter or variable name.

### [AV1706](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1706): Don't use abbreviations (MEDIUM)
- [x] This analyzer reports member, parameter and variable names that consist of a single-letter or contain any of the abbreviations "Btn", "Ctrl", "Frm", "Chk", "Cmb", "Ctx", "Dg", "Pnl", "Dlg", "Lbl", "Txt", "Mnu", "Prg", "Rb", "Cnt", "Tv", "Ddl", "Fld", "Lnk", "Img", "Lit", "Vw", "Gv", "Dts", "Rpt", "Vld", "Pwd", "Ctl", "Tm", "Mgr", "Flt", "Len", "Idx", "Str".

### [AV1708](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1708): Name types using nouns, noun phrases or adjective phrases (MEDIUM)
- [x] This analyzer reports types that have any of the terms "Utility", "Utilities", "Facility", "Facilities", "Helper", "Helpers", "Common" or "Shared" in their name.

### [AV1710](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1710): Don't repeat the name of a class or enumeration in its members (HIGH)
- [x] This analyzer reports members whose name contains the name of their containing type.

### [AV1711](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1711): Name members similarly to members of related .NET Framework classes (LOW)
- [x] This analyzer reports members that are named "AddItem", "Delete" or "NumberOfItems". The other scenarios are already covered by [CA1726](https://msdn.microsoft.com/en-us/library/ms182258.aspx).

### [AV1712](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1712): Avoid short names or names that can be mistaken for other names (HIGH)
- [x] This analyzer reports variables and parameters that are named "b001", "lo", "I1" or "lOl".

### [AV1715](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1715): Properly name properties (MEDIUM)
- [x] This analyzer reports boolean variables, parameters and fields whose names do not start with words like "Is", "Has", "Can" etc.

### [AV1738](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1738): Prefix an event handler with On (LOW)
- [x] This analyzer reports when the target method in an event handler assignment does not match the pattern "On" followed by target field name and the name of the event.

### [AV1739](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1739): Use an underscore for irrelevant lambda parameters (LOW)
- [x] This analyzer reports unused lambda parameters whose names contain any characters other than underscores.

### [AV1745](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1745): Group extension methods in a class suffixed with Extensions (LOW)
- [x] This analyzer reports extension method container classes with a name that does not end in "Extensions".

### [AV1755](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md#av1755): Post-fix asynchronous methods with Async or TaskAsync (MEDIUM)
- [x] This analyzer reports `async` methods whose names do not end with "Async" or "TaskAsync".

## Category: Framework

### [AV2210](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2200_FrameworkGuidelines.md#av2210): Build with the highest warning level (HIGH)
- [x] This analyzer reports when the compiler warning level is lower than 4. Requires [Full Solution Analysis](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Full Solution Analysis.md).

### [AV2215](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2200_FrameworkGuidelines.md#av2215): Properly fill the attributes of the AssemblyInfo.cs file (LOW)
- [x] This analyzer reports when the `AssemblyTitle`, `AssemblyDescription`, `AssemblyConfiguration`, `AssemblyCompany`, `AssemblyProduct`, `AssemblyCopyright`, `AssemblyTrademark`, `AssemblyVersion` or `AssemblyFileVersion` is missing or invoked with an empty string.

### [AV2230](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2200_FrameworkGuidelines.md#av2230): Only use the dynamic keyword when talking to a dynamic object (HIGH)
- [x] This analyzer reports when a member, parameter or variable that is declared as `dynamic` is assigned the result of an expression whose type is not `object` or `dynamic`.

### [AV2235](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2200_FrameworkGuidelines.md#av2235): Favor async/await over the Task (HIGH)
- [x] This analyzer reports invocations of `Task.ContinueWith`.

## Category: Documentation

### [AV2305](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2300_DocumentationGuidelines.md#av2305): Document all public, protected and internal types and members (MEDIUM)
- [x] This analyzer reports missing XML documentation comments on internal types, members and parameters when compiling with documentation comments enabled. The other scenarios (public and protected types and members) are already covered by the C# compiler.

### [AV2310](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2300_DocumentationGuidelines.md#av2310): Avoid inline comments (MEDIUM)
- [x] This analyzer reports single-line and multi-line comments inside method bodies.

### [AV2318](https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2300_DocumentationGuidelines.md#av2318): Don't use comments for tracking work to be done later (LOW)
- [x] This analyzer reports comments that start with the word "TODO".

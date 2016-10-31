# Implementation overview
The list below describes per rule what its analyzer reports on. Checked rules are implemented.

## Category: Class Design

### AV1000: A class or interface should have a single purpose (HIGH)
- [x] This analyzer reports types that have the word "And" in their name.

### AV1008: Avoid static classes (LOW)
- [x] This analyzer reports static classes with a name that does not end in "Extensions" and public/internal non-extension methods.

### AV1010: Don't hide inherited members with the new keyword (HIGH)
- [x] This analyzer reports members that have the `new` modifier in their signature.

### AV1115: A method or property should do only one thing (HIGH)
- [x] This analyzer reports members that have the word "And" in their name.

### AV1125: Don't expose stateful objects through static members (MEDIUM)
- [ ] This analyzer reports static property getters whose method body contains an execution path that returns a type that has non-static members.

## Category: Member Design

### AV1130: Return an IEnumerable<T> or ICollection<T> instead of a concrete collection class (MEDIUM)
- [x] This analyzer reports members whose signature returns a `class` or `struct` that implements `IEnumerable`.

## Category: Miscellaneous Design

### AV1225: Use a protected virtual method to raise each event (MEDIUM)
- [x] This analyzer reports methods that invoke an event, which are non-protected or non-virtual or their name does not end with the word "On" followed by the event name. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1235: Don't pass null as the sender argument when raising an event (HIGH)
- [x] This analyzer reports when a non-static event is invoked with `null` for the "sender" parameter, or when an event is invoked with `null` for the "args" parameter. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1250: Evaluate the result of a LINQ expression before returning it (HIGH)
- [x] This analyzer reports when the result of a LINQ method that uses deferred execution is returned. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

## Category: Maintainability

### AV1500: Methods should not exceed 7 statements (HIGH)
- [x] This analyzer reports methods that consist of more than seven statements. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1502: Avoid conditions with double negatives (MEDIUM)
- [x] This analyzer reports usages of the logical not operator with an argument that contains the word "No" or "Not" in it. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1505: Name assemblies after their contained namespace (LOW)
- [x] This analyzer reports namespaces and types that do not match with the assembly name, unless the assembly name ends with ".Core".

### AV1507: Limit the contents of a source code file to one type (LOW)
- [ ] This analyzer reports when a file contains multiple non-nested types.

### AV1522: Assign each variable in a separate statement (HIGH)
- [x] This analyzer reports when multiple properties, fields, parameters or variables are assigned in a single statement. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1525: Don't make explicit comparisons to true or false (HIGH)
- [x] This analyzer reports boolean comparisons with `true` or `false`. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1530: Don't change a loop variable inside a for loop (MEDIUM)
- [x] This analyzer reports when loop variables are assigned in the loop body.

### AV1532: Avoid nested loops (MEDIUM)
- [x] This analyzer reports when `for`, `foreach`, `while` or `do-while` loops are nested. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1535: Always add a block after keywords such as if, else, while, for, foreach and case (MEDIUM)
- [x] This analyzer reports when a case statement does not have a block. The other scenarios are already covered by Resharper. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1536: Always add a default block after the last case in a switch statement (HIGH)
- [x] This analyzer reports when `switch` statements on (nullable) `bool` or (nullable) non-flags `enum` types are incomplete and do not have a default case. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1537: Finish every if-else-if statement with an else-part (MEDIUM)
- [ ] This analyzer reports `if-else-if` statements that do not end with an unconditional `else` clause.

### AV1551: Call the more overloaded method from other overloads (MEDIUM)
- [x] This analyzer reports when the method overload with the most parameters is not declared virtual. For the other overloads, it reports if they do not delegate to another overload or when their parameter order is different. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1555: Avoid using named arguments (HIGH)
- [x] This analyzer reports invocations with named arguments that are not of type `bool` or `bool?`. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1561: Don't allow methods and constructors with more than three parameters (HIGH)
- [x] This analyzer reports methods, indexers and delegates that declare more than three parameters.

### AV1564: Avoid methods that take a bool flag (HIGH)
- [x] This analyzer reports methods that declare parameters of type `bool` or `bool?`.

### AV1568: Don't use parameters as temporary variables (LOW)
- [x] This analyzer reports parameters that are written to and are not declared as `ref` or `out`.

## Category: Naming

### AV1704: Don't include numbers in variables, parameters and type members (LOW)
- [x] This analyzer reports when a digit occurs in a type, member, parameter or variable name. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1706: Don't use abbreviations (MEDIUM)
- [x] This analyzer reports member, parameter and variable names that consist of a single-letter or contain any of the abbreviations "Btn", "Ctrl", "Frm", "Chk", "Cmb", "Ctx", "Dg", "Pnl", "Dlg", "Lbl", "Txt", "Mnu", "Prg", "Rb", "Cnt", "Tv", "Ddl", "Fld", "Lnk", "Img", "Lit", "Vw", "Gv", "Dts", "Rpt", "Vld", "Pwd", "Ctl", "Tm", "Mgr", "Flt", "Len", "Idx", "Str". Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1708: Name types using nouns, noun phrases or adjective phrases (MEDIUM)
- [x] This analyzer reports types that have any of the terms "Utility", "Utilities", "Facility", "Facilities", "Helper", "Helpers", "Common" or "Shared" in their name.

### AV1710: Don't repeat the name of a class or enumeration in its members (HIGH)
- [x] This analyzer reports members whose name contains the name of their containing type.

### AV1711: Name members similarly to members of related .NET Framework classes (LOW)
- [ ] This analyzer reports members that are named "AddItem", "Delete" or "NumberOfItems". The other scenarios are already covered by [CA1726](https://msdn.microsoft.com/en-us/library/ms182258.aspx).

### AV1712: Avoid short names or names that can be mistaken for other names (HIGH)
- [x] This analyzer reports variables and parameters that are named "b001", "lo", "I1" or "lOl". Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV1715: Properly name properties (MEDIUM)
- [ ] This analyzer reports boolean variables, parameters and fields whose names do not start with the word "Is", "Has", "Allows", "Can", "Should", "Will", "Do" or "Supports".

### AV1738: Prefix an event handler with On (LOW)
- [ ] This analyzer reports event handler assignments, in case the assigned method name is not suffixed with "On" followed by the name of the event.

### AV1739: Use an underscore for irrelevant lambda parameters (LOW)
- [ ] This analyzer reports unused lambda parameters whose names contain any characters other than underscores.

### AV1745: Group extension methods in a class suffixed with Extensions (LOW)
- [x] This analyzer reports extension method container classes with a name that does not end in "Extensions".

### AV1755: Post-fix asynchronous methods with Async or TaskAsync (MEDIUM)
- [x] This analyzer reports `async` methods whose names do not end with "Async" or "TaskAsync".

## Category: Framework

### AV2210: Build with the highest warning level (HIGH)
- [x] This analyzer reports when the compiler warning level is lower than 4.

### AV2215: Properly fill the attributes of the AssemblyInfo.cs file (LOW)
- [ ] This analyzer reports when the `AssemblyTitle`, `AssemblyDescription`, `AssemblyConfiguration`, `AssemblyCompany`, `AssemblyProduct`, `AssemblyCopyright`, `AssemblyTrademark`, `AssemblyCulture`, `AssemblyVersion` or `AssemblyFileVersion` is missing or invoked with an empty string.

### AV2230: Only use the dynamic keyword when talking to a dynamic object (HIGH)
- [x] This analyzer reports when a member, parameter or variable that is declared as `dynamic` is assigned the result of an expression whose type is not `object` or `dynamic`. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

### AV2235: Favor async/await over the Task (HIGH)
- [x] This analyzer reports invocations of `Task.ContinueWith`. Requires [IOperation support](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/IOperation.md).

## Category: Documentation

### AV2305: Document all public, protected and internal types and members (MEDIUM)
- [ ] This analyzer reports missing XML documentation comments on internal types and members. The other scenarios (public and protected types and members) are already covered by the C# compiler.

### AV2310: Avoid inline comments (MEDIUM)
- [x] This analyzer reports single-line and multi-line comments inside method bodies.

### AV2318: Don't use comments for tracking work to be done later (LOW)
- [ ] This analyzer reports comments that start with the word "TODO".

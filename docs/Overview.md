# Implementation overview
The list below describes per rule what its analyzer reports on.

## Category: Class Design

### [AV1000](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1000_ClassDesignGuidelines.md#av1000): A class or interface should have a single purpose ![](/images/warn.png "severity: warning")
This analyzer reports when a type has the word "And" in its name.

### [AV1008](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1000_ClassDesignGuidelines.md#av1008): Avoid static classes ![](/images/info.png "severity: info")
This analyzer reports when:
- the name of a static class does not end in "Extensions"
- a static class whose name ends in "Extensions" contains a public or internal non-extension method.

### [AV1010](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1000_ClassDesignGuidelines.md#av1010): Don't suppress compiler warnings using the `new` keyword ![](/images/warn.png "severity: warning")
This analyzer reports when a member has the `new` modifier in its signature.

## Category: Member Design

### [AV1115](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1100_MemberDesignGuidelines.md#av1115): A property, method or local function should do only one thing ![](/images/warn.png "severity: warning")
This analyzer reports when a member has the word "And" in its name.

### [AV1130](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1100_MemberDesignGuidelines.md#av1130): Return an `IEnumerable<T>` or `ICollection<T>` instead of a concrete collection class ![](/images/warn.png "severity: warning")
This analyzer reports when a method return type is a `class` or `struct` that implements `IEnumerable` and is not an immutable collection.

## Category: Miscellaneous Design

### [AV1210](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1200_MiscellaneousDesignGuidelines.md#av1210): Don't swallow errors by catching generic exceptions ![](/images/warn.png "severity: warning") 
This analyzer reports when a handler catches `Exception`, `SystemException` or `ApplicationException` without a `when` filter.

### [AV1225](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1200_MiscellaneousDesignGuidelines.md#av1225): Use a protected virtual method to raise each event ![](/images/warn.png "severity: warning")
This analyzer reports when an event is raised:
- not from a method (for example, from a lambda expression or local function)
- from a method that is not protected and virtual
- from a method whose name does not match the pattern On{EventName}, for example: OnValueChanged.

### [AV1235](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1200_MiscellaneousDesignGuidelines.md#av1235): Don't pass `null` as the `sender` argument when raising an event ![](/images/warn.png "severity: warning")
This analyzer reports when:
- 'sender' argument is null when raising a non-static event
- 'args' argument is null when raising an event.

### [AV1250](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1200_MiscellaneousDesignGuidelines.md#av1250): Evaluate the result of a LINQ expression before returning it ![](/images/warn.png "severity: warning")
This analyzer reports when a method return type is `IEnumerable` or `IEnumerable<T>` and it returns:
- the result of a method call that uses deferred execution (for example: `Where`, `Select`, `Concat`)
- the result of a query (LINQ expression)
- an expression of type `IQueryable` or `IQueryable<T>`.

## Category: Maintainability

### [AV1500](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1500): Methods should not exceed 7 statements ![](/images/warn.png "severity: warning")
This analyzer reports when a method body (such as a method, property getter or local function) contains more than 7 statements.

### [AV1502](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1502): Avoid conditions with double negatives ![](/images/warn.png "severity: warning")
This analyzer reports when the logical not operator is applied on an argument that has the word "No" or "Not" in its name.

### [AV1505](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1505): Name assemblies after their contained namespace ![](/images/info.png "severity: info")
This analyzer reports when:
- a namespace does not match with the assembly name
- a type is declared in a namespace that does not match with the assembly name
- a type is not declared in a namespace.

### [AV1506](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1506): Name a source file to the type it contains ![](/images/info.png "severity: info")
This analyzer reports when:
- a file is not named using pascal casing
- a file name contains an underscore
- a file name includes generic arity.

### [AV1507](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1507): Limit the contents of a source code file to one type ![](/images/info.png "severity: info")
This analyzer reports when a file contains multiple non-nested types, unless they only differ by generic arity.

### [AV1522](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1522): Assign each variable in a separate statement ![](/images/warn.png "severity: warning")
This analyzer reports when multiple properties, fields, parameters or variables are assigned in a single statement, except when using out variables, is-patterns or deconstruction into tuples.

### [AV1530](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1530): Don't change a loop variable inside a `for` loop ![](/images/warn.png "severity: warning")
This analyzer reports when a `for` loop variable is written to in the loop body.

### [AV1532](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1532): Avoid nested loops ![](/images/warn.png "severity: warning")
This analyzer reports when `for`, `foreach`, `while` or `do-while` loops are nested.

### [AV1535](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1535): Always add a block after the keywords `if`, `else`, `do`, `while`, `for`, `foreach` and `case` ![](/images/warn.png "severity: warning")
This analyzer reports when a `case` or `default` clause in a switch statement does not have a block. The other keywords can be configured using Resharper.

### [AV1536](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1536): Always add a `default` block after the last `case` in a `switch` statement ![](/images/warn.png "severity: warning")
This analyzer reports when a `switch` statement on a (nullable) `bool` or (nullable) non-flags `enum` type is incomplete and does not have a `default` clause.

### [AV1537](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1537): Finish every `if`-`else`-`if` statement with an `else` clause ![](/images/warn.png "severity: warning")
This analyzer reports when an `if`-`else`-`if` construct does not end with an unconditional `else` clause.

### [AV1551](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1551): Call the more overloaded method from other overloads ![](/images/warn.png "severity: warning")
This analyzer reports when:
- an overloaded method does not call another overload (unless it is the longest in the group)
- the longest overloaded method (the one with the most parameters) is not virtual
- the order of parameters in an overloaded method does not match with the parameter order of the longest overload.

### [AV1555](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1555): Avoid using named arguments ![](/images/warn.png "severity: warning")
This analyzer reports when a named argument is used, unless the parameter type is `bool` or `bool?`.

### [AV1561](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1561): Don't declare signatures with more than 3 parameters ![](/images/warn.png "severity: warning")
This analyzer reports when a method, constructor, local function, indexer or delegate:
- declares more than three parameters
- declares a tuple parameter
- returns a tuple with more than 2 elements.

### [AV1562](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1562): Don't use ref or out parameters ![](/images/warn.png "severity: warning")
This analyzer reports when:
- a parameter is declared as `ref`
- a parameter is declared as `out`, unless the containing method name starts with "Try".

### [AV1564](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1564): Avoid signatures that take a `bool` parameter ![](/images/warn.png "severity: warning")
This analyzer reports when a public or internal member declares a parameter of type `bool` or `bool?`.

### [AV1568](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1500_MaintainabilityGuidelines.md#av1568): Don't use parameters as temporary variables ![](/images/info.png "severity: info")
This analyzer reports when a by-value (not `ref`, `in` or `out`) parameter is written to.

## Category: Naming

### [AV1704](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1704): Don't include numbers in variables, parameters and type members ![](/images/info.png "severity: info")
This analyzer reports when a digit occurs in the name of a type, member, local function, parameter, tuple element or variable.

### [AV1706](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1706): Don't use abbreviations ![](/images/warn.png "severity: warning")
This analyzer reports when the name of a type, member, local function, parameter, tuple element or variable consists of a single letter or contains an abbreviation like "Btn", "Ctrl", "Frm", "Chk", "Str" etc.

### [AV1708](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1708): Name types using nouns, noun phrases or adjective phrases ![](/images/warn.png "severity: warning")
This analyzer reports when a type name contains the term "Utility", "Utilities", "Facility", "Facilities", "Helper", "Helpers", "Common" or "Shared".

### [AV1710](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1710): Don't repeat the name of a class or enumeration in its members ![](/images/warn.png "severity: warning")
This analyzer reports when a member name contains the name of its containing type.

### [AV1711](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1711): Name members similarly to members of related .NET Framework classes ![](/images/info.png "severity: info")
This analyzer reports when a member is named "AddItem", "Delete" or "NumberOfItems". See also [CA1726](https://msdn.microsoft.com/en-us/library/ms182258.aspx).

### [AV1712](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1712): Avoid short names or names that can be mistaken for other names ![](/images/warn-disabled.png "severity: warning; disabled by default")
This analyzer reports when a variable or parameter is named "b001", "lo", "I1" or "lOl".

### [AV1715](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1715): Properly name properties ![](/images/warn-disabled.png "severity: warning; disabled by default")
This analyzer reports when a public or internal (nullable) boolean member or parameter name does not start with a word like "is", "has", "can" etc.

### [AV1738](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1738): Prefix an event handler with "On" ![](/images/info.png "severity: info")
This analyzer reports when the name of an event handler method does not match the pattern {TargetName}On{EventName}, for example: OkButtonOnClick.

### [AV1739](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1739): Use an underscore for irrelevant lambda parameters ![](/images/info.png "severity: info")
This analyzer reports when an unused lambda parameter name does not consist solely of underscores.

### [AV1745](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1745): Group extension methods in a class suffixed with Extensions ![](/images/info.png "severity: info")
This analyzer reports when all public and internal methods of a static class are extension methods, but its name does not end with "Extensions".

### [AV1755](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/1700_NamingGuidelines.md#av1755): Postfix asynchronous methods with `Async` or `TaskAsync` ![](/images/warn.png "severity: warning")
This analyzer reports when the name of an `async` method does not end with "Async".

## Category: Framework

### [AV2202](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/2200_FrameworkGuidelines.md#av2202): Prefer language syntax over explicit calls to underlying implementations ![](/images/warn.png "severity: warning") 
This analyzer reports when:
- the `HasValue` property of a nullable value type is used to check for `null`
- a comparison with a nullable value type contains a redundant null check.

### [AV2210](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/2200_FrameworkGuidelines.md#av2210): Build with the highest warning level ![](/images/warn.png "severity: warning")
This analyzer reports when:
- the compiler warning level is below 4 in the build settings
- warnings are not treated as errors in the build settings.

Note: this analyzer requires [Full Solution Analysis](https://github.com/bkoelman/CSharpGuidelinesAnalyzer/blob/master/docs/Full%20Solution%20Analysis.md) enabled.

### [AV2220](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/2200_FrameworkGuidelines.md#av2220): Avoid LINQ for simple expressions ![](/images/info.png "severity: info")
This analyzer reports when a query (LINQ expression) can be changed into a single method call.

### [AV2230](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/2200_FrameworkGuidelines.md#av2230): Only use the `dynamic` keyword when talking to a dynamic object ![](/images/warn.png "severity: warning")
This analyzer reports when a statically typed expression is implicitly converted to `dynamic`, unless the source type is `object` or `ObjectHandle`.

### [AV2235](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/2200_FrameworkGuidelines.md#av2235): Favor `async`/`await` over `Task` continuations ![](/images/warn.png "severity: warning")
This analyzer reports when `Task.ContinueWith` is called.

## Category: Documentation

### [AV2305](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/2300_DocumentationGuidelines.md#av2305): Document all `public`, `protected` and `internal` types and members ![](/images/warn.png "severity: warning")
This analyzer reports (in addition to what the C# compiler reports) when compiling with documentation comments enabled and:
- an internal type has no XML documentation comments
- an internal member has no XML documentation comments
- a public member in an internal type has no XML documentation comments
- a parameter of an internal member has no XML documentation comments.

### [AV2310](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/2300_DocumentationGuidelines.md#av2310): Avoid inline comments ![](/images/warn-disabled.png "severity: warning; disabled by default")
This analyzer reports when a comment in found inside a method body, except when it is a Resharper suppression or language injection comment.

### [AV2318](https://github.com/dennisdoomen/CSharpGuidelines/blob/a7edaf8d516b7a129e5e4967bd51e9012228e479/_pages/2300_DocumentationGuidelines.md#av2318): Don't use comments for tracking work to be done later ![](/images/info.png "severity: info")
This analyzer reports when a comment starts with the word "TODO".

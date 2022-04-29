# Implementation overview
The list below describes per rule what its analyzer reports on.

## Category: Class Design

### [AV1000](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1000.md): A class or interface should have a single purpose ![](/images/warn.png "severity: warning")
This analyzer reports when a type has the word "And" in its name.

### [AV1008](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1008.md): Avoid static classes ![](/images/info.png "severity: info")
This analyzer reports when:
- the name of a static class does not end in "Extensions"
- a static class whose name ends in "Extensions" contains a public or internal non-extension method.

### [AV1010](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1010.md): Don't suppress compiler warnings using the `new` keyword ![](/images/warn.png "severity: warning")
This analyzer reports when a member has the `new` modifier in its signature.

## Category: Member Design

### [AV1115](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1115.md): A property, method or local function should do only one thing ![](/images/warn.png "severity: warning")
This analyzer reports when a member has the word "And" in its name.

### [AV1130](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1130.md): Return an `IEnumerable<T>` or `ICollection<T>` instead of a concrete collection class ![](/images/warn.png "severity: warning")
This analyzer reports when the return type of a public or internal method implements `IEnumerable` and is changeable (for example: `List<string>` or `ICollection<int>`). Instead, return `IEnumerable<T>`, `IAsyncEnumerable<T>`, `IQueryable<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`, `IReadOnlySet<T>`, `IReadOnlyDictionary<TKey, TValue>` or an immutable collection.

### [AV1135](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1135.md): Properties, arguments and return values representing strings or collections should never be `null` ![](/images/warn.png "severity: warning")
This analyzer reports when `null` is returned from a method, local function, lambda expression or property getter which has a return type of string, collection or task.

## Category: Miscellaneous Design

### [AV1210](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1210.md): Don't swallow errors by catching generic exceptions ![](/images/warn.png "severity: warning")
This analyzer reports when a handler catches `Exception`, `SystemException` or `ApplicationException` without a `when` filter.

### [AV1225](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1225.md): Use a protected virtual method to raise each event ![](/images/warn.png "severity: warning")
This analyzer reports when an event is raised:
- not from a method (for example, from a lambda expression or local function)
- from a method that is not protected and virtual
- from a method whose name does not match the pattern On{EventName}, for example: OnValueChanged.

### [AV1235](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1235.md): Don't pass `null` as the `sender` argument when raising an event ![](/images/warn.png "severity: warning")
This analyzer reports when:
- 'sender' argument is null when raising a non-static event
- 'args' argument is null when raising an event.

### [AV1250](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1250.md): Evaluate the result of a LINQ expression before returning it ![](/images/warn.png "severity: warning")
This analyzer reports when the return type of a public method is `IEnumerable` or `IEnumerable<T>` and it returns:
- the result of a method call that uses deferred execution (for example: `Where`, `Select`, `Concat`)
- the result of a query (LINQ expression)
- an expression of type `IQueryable` or `IQueryable<T>`.

## Category: Maintainability

### [AV1500](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1500.md): Methods should not exceed 7 statements ![](/images/warn.png "severity: warning")
This analyzer reports when a method body (such as a method, property getter or local function) contains more than 7 statements.

**Note:** This rule can be customized using a [configuration file](/docs/Configuration.md) by setting **MaxStatementCount** to a value in range 0-255.

### [AV1502](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1502.md): Avoid conditions with double negatives ![](/images/warn.png "severity: warning")
This analyzer reports when the logical not operator is applied on an argument that has the word "No" or "Not" in its name.

### [AV1505](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1505.md): Name assemblies after their contained namespace ![](/images/info.png "severity: info")
This analyzer reports when:
- a namespace does not match with the assembly name
- a type is declared in a namespace that does not match with the assembly name
- a type is not declared in a namespace.

### [AV1506](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1506.md): Name a source file to the type it contains ![](/images/info.png "severity: info")
This analyzer reports when:
- a file is not named using pascal casing
- a file name contains an underscore
- a file name includes generic arity.

### [AV1507](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1507.md): Limit the contents of a source code file to one type ![](/images/info.png "severity: info")
This analyzer reports when a file contains multiple non-nested types, unless they only differ by generic arity.

### [AV1522](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1522.md): Assign each variable in a separate statement ![](/images/warn.png "severity: warning")
This analyzer reports when multiple properties, fields, parameters or variables are assigned in a single statement, except when using out variables, is-patterns or deconstruction into tuples.

### [AV1530](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1530.md): Don't change a loop variable inside a `for` loop ![](/images/warn.png "severity: warning")
This analyzer reports when a `for` loop variable is written to in the loop body.

### [AV1532](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1532.md): Avoid nested loops ![](/images/warn.png "severity: warning")
This analyzer reports when `for`, `foreach`, `while` or `do-while` loops are nested.

### [AV1535](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1535.md): Always add a block after the keywords `if`, `else`, `do`, `while`, `for`, `foreach` and `case` ![](/images/warn.png "severity: warning")
This analyzer reports when a `case` or `default` clause in a switch statement does not have a block. The other keywords can be configured using Resharper.

### [AV1536](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1536.md): Always add a `default` block after the last `case` in a `switch` statement ![](/images/warn.png "severity: warning")
This analyzer reports when a non-exhaustive `switch` statement on a (nullable) `bool` or (nullable) non-flags `enum` type does not have a `default` clause.

### [AV1537](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1537.md): Finish every `if`-`else`-`if` statement with an `else` clause ![](/images/warn.png "severity: warning")
This analyzer reports when an `if`-`else`-`if` construct does not end with an unconditional `else` clause.

### [AV1551](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1551.md): Call the more overloaded method from other overloads ![](/images/warn.png "severity: warning")
This analyzer reports when:
- an overloaded method does not call another overload (unless it is the longest in the group)
- the longest overloaded method (the one with the most parameters) is not virtual
- the order of parameters in an overloaded method does not match with the parameter order of the longest overload.

### [AV1553](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1553.md): Only use optional parameters to replace overloads ![](/images/warn.png "severity: warning")
This analyzer reports when an optional parameter of type string, collection or task has default value `null`.

### [AV1554](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1554.md): Do not use optional parameters in interface methods or their concrete implementations ![](/images/warn.png "severity: warning")
This analyzer reports when an interface method or an abstract/virtual/override method contains an optional parameter.

### [AV1555](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1555.md): Avoid using named arguments ![](/images/warn.png "severity: warning")
This analyzer reports when a named argument is used, unless the parameter type is `bool` or `bool?`.

### [AV1561](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1561.md): Don't declare signatures with more than 3 parameters ![](/images/warn.png "severity: warning")
This analyzer reports when a method, constructor, local function, indexer or delegate:
- declares more than three parameters
- declares a tuple parameter
- returns a tuple with more than 2 elements.

**Note:** This rule can be customized using a [configuration file](/docs/Configuration.md) by setting **MaxParameterCount** and/or **MaxConstructorParameterCount** to a value in range 0-255.
When **MaxConstructorParameterCount** is omitted, the value from **MaxParameterCount** (or its default) is used for constructors.

### [AV1562](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1562.md): Don't use ref or out parameters ![](/images/warn.png "severity: warning")
This analyzer reports when:
- a parameter is declared as `ref`
- a parameter is declared as `out`, unless the containing method name starts with "Try".

### [AV1564](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1564.md): Avoid signatures that take a `bool` parameter ![](/images/warn.png "severity: warning")
This analyzer reports when a public or internal member declares a parameter of type `bool` or `bool?`.

### [AV1568](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1568.md): Don't use parameters as temporary variables ![](/images/info.png "severity: info")
This analyzer reports when a by-value (not `ref`, `in` or `out`) parameter is written to.

### [AV1580](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1580.md): Write code that is easy to debug ![](/images/warn.png "severity: warning")
This analyzer reports using nested method calls.

## Category: Naming

### [AV1704](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1704.md): Don't include numbers in variables, parameters and type members ![](/images/info.png "severity: info")
This analyzer reports when a digit occurs in the name of a type, member, local function, parameter, tuple element or variable.

### [AV1706](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1706.md): Don't use abbreviations ![](/images/warn.png "severity: warning")
This analyzer reports when the name of a type, member, local function, parameter, tuple element or variable consists of a single letter or contains an abbreviation like "Btn", "Ctrl", "Frm", "Chk", "Str" etc.

### [AV1708](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1708.md): Name types using nouns, noun phrases or adjective phrases ![](/images/warn.png "severity: warning")
This analyzer reports when a type name contains the term "Utility", "Utilities", "Facility", "Facilities", "Helper", "Helpers", "Common" or "Shared".

### [AV1710](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1710.md): Don't repeat the name of a class or enumeration in its members ![](/images/warn.png "severity: warning")
This analyzer reports when a member name contains the name of its containing type.

### [AV1711](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1711.md): Name members similarly to members of related .NET Framework classes ![](/images/info.png "severity: info")
This analyzer reports when a member is named "AddItem", "Delete" or "NumberOfItems". See also [CA1726](https://msdn.microsoft.com/en-us/library/ms182258.aspx).

### [AV1712](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1712.md): Avoid short names or names that can be mistaken for other names ![](/images/warn-disabled.png "severity: warning; disabled by default")
This analyzer reports when a variable or parameter is named "b001", "lo", "I1" or "lOl".

### [AV1715](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1715.md): Properly name properties ![](/images/warn-disabled.png "severity: warning; disabled by default")
This analyzer reports when a public or internal (nullable) boolean member or parameter name does not start with a word like "is", "has", "can" etc.

### [AV1738](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1738.md): Prefix an event handler with "On" ![](/images/info.png "severity: info")
This analyzer reports when the name of an event handler method does not match the pattern {TargetName}On{EventName}, for example: OkButtonOnClick.

### [AV1739](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1739.md): Use an underscore for irrelevant lambda parameters ![](/images/info.png "severity: info")
This analyzer reports when an unused lambda parameter name does not consist solely of underscores.

### [AV1745](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1745.md): Group extension methods in a class suffixed with Extensions ![](/images/info.png "severity: info")
This analyzer reports when all public and internal methods of a static class are extension methods, but its name does not end with "Extensions".

### [AV1755](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1755.md): Postfix asynchronous methods with `Async` or `TaskAsync` ![](/images/warn.png "severity: warning")
This analyzer reports when the name of an `async` method does not end with "Async".

## Category: Performance

### [AV1840](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/1840.md): Await `ValueTask` and `ValueTask<T>` directly and exactly once ![](/images/warn.png "severity: warning")
This analyzer reports when an expression of type `ValueTask` or `ValueTask<T>` is being assigned or used as argument without `await`.

## Category: Framework

### [AV2202](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2202.md): Prefer language syntax over explicit calls to underlying implementations ![](/images/warn.png "severity: warning")
This analyzer reports when:
- the `HasValue` property of a nullable value type is used to check for `null`
- a comparison with a nullable value type contains a redundant null check.

### [AV2210](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2210.md): Build with the highest warning level ![](/images/warn.png "severity: warning")
This analyzer reports when warnings are not treated as errors in the build settings.

Note: this analyzer requires [Full Solution Analysis](/docs/Full%20Solution%20Analysis.md) enabled.

### [AV2220](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2220.md): Avoid LINQ query syntax for simple expressions ![](/images/info.png "severity: info")
This analyzer reports when a query (LINQ expression) can be changed into a single method call.

### [AV2230](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2230.md): Only use the `dynamic` keyword when talking to a dynamic object ![](/images/warn.png "severity: warning")
This analyzer reports when a statically typed expression is implicitly converted to `dynamic`, unless the source type is `object` or `ObjectHandle`.

### [AV2235](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2235.md): Favor `async`/`await` over `Task` continuations ![](/images/warn.png "severity: warning")
This analyzer reports when `Task.ContinueWith` is called.

## Category: Documentation

### [AV2305](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2305.md): Document all `public`, `protected` and `internal` types and members ![](/images/warn.png "severity: warning")
This analyzer reports (in addition to what the C# compiler reports) when compiling with documentation comments enabled and:
- an internal type has no XML documentation comments
- an internal member has no XML documentation comments
- a public member in an internal type has no XML documentation comments
- a parameter of an internal member has no XML documentation comments.

### [AV2310](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2310.md): Avoid inline comments ![](/images/warn-disabled.png "severity: warning; disabled by default")
This analyzer reports when a comment is found inside a method body, except when it is a Resharper [suppression](https://www.jetbrains.com/help/resharper/Code_Analysis__Configuring_Warnings.html#suppress), [language injection](https://blog.jetbrains.com/dotnet/2016/12/26/language-injections-in-resharper-ultimate-2016-3/) or [formatter configuration](https://blog.jetbrains.com/dotnet/2017/11/27/different-code-styles-different-code-blocks-resharper-rider/) comment.

### [AV2318](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2318.md): Don't use comments for tracking work to be done later ![](/images/info.png "severity: info")
This analyzer reports when a comment starts with the word "TODO".

## Category: Layout

### [AV2407](https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/2407.md): Do not use #region ![](/images/warn.png "severity: warning")
This analyzer reports when a `#region` directive is found.

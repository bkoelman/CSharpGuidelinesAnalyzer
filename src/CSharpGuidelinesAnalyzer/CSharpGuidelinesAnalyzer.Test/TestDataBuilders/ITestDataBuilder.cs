namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

internal interface ITestDataBuilder<out T>
{
    // ReSharper disable once UnusedMemberInSuper.Global
    T Build();
}

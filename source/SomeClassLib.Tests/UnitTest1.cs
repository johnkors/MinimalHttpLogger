namespace SomeClassLib.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Assert.Equal("Hello, JOHN", Class1.Greet("John"));
    }
}

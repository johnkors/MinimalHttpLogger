using SomePeerDependency;

namespace SomeClassLib;

public class Class1
{
    public static string Greet(string name)
    {
        return $"Hello, {PeerClass.Upper(name)}";
    }
}

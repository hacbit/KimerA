namespace KimerA.Analysis.Test
{
    [Stringify]
    public sealed partial class TestClass(string name, int age)
    {
        public string Name { get; set; } = name;

        public int Age { get; set; } = age;
    }

    public record TestRecord(string Name, int Age);

    public record TestRecordClass
    {
        public string Name;
    }

    public record Foo(string Bar)
    {
        public required string R { get; init; }
    }
}

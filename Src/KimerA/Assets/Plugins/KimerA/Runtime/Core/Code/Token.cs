namespace KimerA.Code
{
    public interface IToken
    {
        CodeType Type { get; }

        string AsToken();
    }

    public sealed class Using : IToken
    {
        public CodeType Type { get; } = CodeType.Using;

        public required IToken Namespace;

        public string AsToken()
        {
            return $"using {Namespace.AsToken()};";
        }
    }

    public sealed class Namespace : IToken
    {
        public CodeType Type { get; } = CodeType.Namespace;

        public required IToken Name;

        public string AsToken()
        {
            return $"namespace {Name.AsToken()}";
        }
    }

    public sealed class Class : IToken
    {
        public CodeType Type { get; } = CodeType.Class;

        public required IToken Name;

        public string AsToken()
        {
            return $"public sealed class {Name.AsToken()}";
        }
    }
}
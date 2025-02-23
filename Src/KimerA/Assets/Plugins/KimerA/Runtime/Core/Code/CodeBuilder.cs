using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.Utilities;

namespace KimerA.Code
{
    /// <summary>
    /// A fluent builder for generating C# code
    /// </summary>
    public sealed class CodeBuilder
    {
        private readonly List<string> m_Usings = new();

        /// <summary>
        /// Add usings to the code
        /// </summary>
        /// <param name="usings"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder AddUsings(params string[] usings)
        {
            foreach (var @using in usings)
            {
                m_Usings.Add($"using {@using};");
            }
            return this;
        }

        private string? m_Namespace;

        /// <summary>
        /// Set the namespace of the code
        /// </summary>
        /// <param name="namespace"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder WithNamespace(string @namespace)
        {
            m_Namespace = @namespace;
            return this;
        }

        public enum SymbolType
        {
            Class,
            Interface,
            Enum,
            Struct
        }

        private SymbolType m_SymbolType = SymbolType.Class;

        private string? m_Name;

        /// <summary>
        /// Set the symbol type and name of the code
        /// </summary>
        /// <param name="symbolType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder WithSymbol(SymbolType symbolType, string name)
        {
            m_SymbolType = symbolType;
            m_Name = name;
            return this;
        }

        /// <summary>
        /// Alias for WithSymbol(SymbolType.Struct, name)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder WithStruct(string name)
        {
            return WithSymbol(SymbolType.Struct, name);
        }

        /// <summary>
        /// Alias for WithSymbol(SymbolType.Class, name)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder WithClass(string name)
        {
            return WithSymbol(SymbolType.Class, name);
        }

        /// <summary>
        /// Alias for WithSymbol(SymbolType.Interface, name)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder WithInterface(string name)
        {
            return WithSymbol(SymbolType.Interface, name);
        }

        /// <summary>
        /// Alias for WithSymbol(SymbolType.Enum, name)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder WithEnum(string name)
        {
            return WithSymbol(SymbolType.Enum, name);
        }

        private readonly List<string> m_BaseTypes = new();

        /// <summary>
        /// Add Inheritance or Interface implementation to the code
        /// </summary>
        /// <param name="baseTypes"></param>
        /// <returns></returns>
        public CodeBuilder WithImplements(params string[] baseTypes)
        {
            m_BaseTypes.AddRange(baseTypes);
            return this;
        }

        public enum AccessModifier
        {
            Public,
            Private,
            Protected,
            Internal,
            ProtectedInternal
        }

        private AccessModifier m_AccessModifier = AccessModifier.Public;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder WithAccessModifier(AccessModifier accessModifier)
        {
            m_AccessModifier = accessModifier;
            return this;
        }

        public enum MemberType
        {
            Field,
            Property,
        }

        public struct Member
        {
            public MemberType Type;
            public string Name;
            public string TypeOrReturnType;
            public string Value;
            public AccessModifier AccessModifier;
        }

        private readonly List<Member> m_Members = new();

        /// <summary>
        /// Add a member to the code
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder AddMember(Member member)
        {
            m_Members.Add(member);
            return this;
        }

        /// <summary>
        /// Add a member to the code
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="typeOrReturnType"></param>
        /// <param name="value"></param>
        /// <param name="accessModifier"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder AddMember(MemberType type, string name, string typeOrReturnType, 
            string value = "", AccessModifier accessModifier = AccessModifier.Public)
        {
            return AddMember(new Member
            {
                Type = type,
                Name = name,
                TypeOrReturnType = typeOrReturnType,
                Value = value,
                AccessModifier = accessModifier
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CodeBuilder AddMembers<T>(IEnumerable<T> values, Func<T, Member> func)
        {
            foreach (var v in values)
            {
                AddMember(func.Invoke(v));
            }
            return this;
        }

        public struct Method
        {
            public string Name;
            public string ReturnType;
            public AccessModifier AccessModifier;
            public string Body;
            public string[] Parameters;
        }

        private readonly List<Method> m_Methods = new();

        public CodeBuilder AddMethod(Method method)
        {
            m_Methods.Add(method);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string BuildAccessModifier(AccessModifier accessModifier)
        {
            return accessModifier switch
            {
                AccessModifier.Public => "public",
                AccessModifier.Private => "private",
                AccessModifier.Protected => "protected",
                AccessModifier.Internal => "internal",
                AccessModifier.ProtectedInternal => "protected internal",
                _ => string.Empty,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string BuildSymbolType(SymbolType symbolType)
        {
            return symbolType switch
            {
                SymbolType.Class => "class",
                SymbolType.Struct => "struct",
                SymbolType.Enum => "enum",
                SymbolType.Interface => "interface",
                _ => string.Empty,
            };
        }

        private string BuildMember(Member member)
        {
            var code = $"{BuildAccessModifier(member.AccessModifier)} {member.TypeOrReturnType} {member.Name}";
            if (member.Value.IsNullOrWhitespace())
            {
                return code + ';';
            }
            else
            {
                return code + member.Type switch
                {
                    MemberType.Field => " = ",
                    MemberType.Property => " => ",
                    _ => string.Empty
                }
                + member.Value
                + ';';
            }
        }

        /// <summary>
        /// Build to string
        /// </summary>
        /// <returns></returns>
        public string Build()
        {
            var code = string.Empty;

            if (m_Usings.Count > 0)
            {
                code += string.Join(Environment.NewLine, m_Usings) + Environment.NewLine;
            }

            var indentSingleton = "    ";
            var indent = string.Empty;

            if (m_Namespace.IsNullOrWhitespace() == false)
            {
                code += $"namespace {m_Namespace}" + Environment.NewLine + "{" + Environment.NewLine;
                indent += indentSingleton;
            }

            code += $"{indent}{BuildAccessModifier(m_AccessModifier)} {BuildSymbolType(m_SymbolType)} {m_Name}";
            if (m_BaseTypes.Count > 0)
            {
                code += " : " + string.Join(", ", m_BaseTypes);
            }
            code += Environment.NewLine
                + '{'
                + Environment.NewLine;

            indent += indentSingleton;

            foreach (var member in m_Members)
            {
                code += indent + BuildMember(member) + Environment.NewLine;
            }

            for (var i = indent.Length / indentSingleton.Length - 1; i > 0; i--)
            {
                code += indent[..(i * indentSingleton.Length)] + '}' + Environment.NewLine;
            }

            code += '}';

            return code;
        }

        /// <summary>
        /// Alias for .Build()
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Build();
    }
}
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace KimerA.Analysis.Base
{
    [Generator(LanguageNames.CSharp)]
    public sealed class Stringify : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var syntaxProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
                ).Where(static m => m is not null);
            
            var compilationAndTypes = context.CompilationProvider.Combine(syntaxProvider.Collect());

            context.RegisterSourceOutput(compilationAndTypes, static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
            => node is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax &&
                baseTypeDeclarationSyntax.AttributeLists.Count > 0;

        private static BaseTypeDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var baseTypeDeclarationSyntax = context.Node as BaseTypeDeclarationSyntax;
            foreach (var attributeListSyntax in baseTypeDeclarationSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (attributeSyntax.Name.ToString() == "Stringify")
                    {
                        return baseTypeDeclarationSyntax;
                    }
                }
            }
            return null;
        }

        public static void Execute(Compilation compilation, ImmutableArray<BaseTypeDeclarationSyntax> types, SourceProductionContext context)
        {
            if (types.IsDefaultOrEmpty) return;

            InitStringifyAttribute(context);

            foreach (var typeDec in types)
            {
                AddStringifyExtension(context, typeDec);
            }
        }

        private static void InitStringifyAttribute(SourceProductionContext context)
        {
            var code = @"using System;
namespace KimerA.Analysis
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public sealed class StringifyAttribute : Attribute
    {

    }
}";
            var sourceText = SourceText.From(code, System.Text.Encoding.UTF8);
            context.AddSource("StringifyAttribute.g.cs", sourceText);
        }

        private static void AddStringifyExtension(SourceProductionContext context, BaseTypeDeclarationSyntax typeDec)
        {
            var namespaceName = typeDec.FirstAncestorOrSelf<NamespaceDeclarationSyntax>().Name.ToString();
            var symbolName = typeDec.Identifier.Text;
            var codeText = typeDec.ToFullString();
            var codeTextNormalizeWhitespace = typeDec.NormalizeWhitespace().ToFullString();
            var code = $@"
namespace {namespaceName}
{{
    public sealed class {symbolName}Stringify
    {{
        public static string Stringify()
        {{
            return @""{codeTextNormalizeWhitespace}"";
        }}

        public static string StringifyOriginal()
        {{
            return @""{codeText}"";
        }}
    }}
}}
";
            var sourceText = SourceText.From(code, System.Text.Encoding.UTF8);
            context.AddSource($"{symbolName}_StringifyExtensions.g.cs", sourceText);
        }
    }
}

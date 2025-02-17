using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace KimerA.Analysis
{
    [Generator(LanguageNames.CSharp)]
    public sealed class PluginBaseInjector : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var syntaxProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => IsTargetType(s),
                    transform: (ctx, _) => GetTargetForGeneration(ctx)
                ).Where(m => m is not null);

            var compilationAndTypes = context.CompilationProvider.Combine(syntaxProvider.Collect());

            context.RegisterSourceOutput(compilationAndTypes, (spc, source) => Execute(source.Left, source.Right, spc));
        }

        public void Execute(Compilation compilation, ImmutableArray<StructDeclarationSyntax> structs, SourceProductionContext context)
        {
            foreach (var structDec in structs)
            {
                var namespaceName = structDec.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()?.Name.ToString();
                var structName = structDec.Identifier.Text;
                var methods = structDec.Members.OfType<MethodDeclarationSyntax>()
                    .Where(m => m.AttributeLists.SelectMany(a => a.Attributes)
                        .Any(a => a.Name.ToString() == "SystemFunc"));
                
                var sourceBuilder = new StringBuilder($@"
using System;
using UnityEngine;
using KimerA.ECS;

namespace {namespaceName}
{{
    public partial struct {structName} : IPlugin
    {{
        void IPlugin.RegisterSystems_Inject(App app)
        {{
");
                foreach (var method in methods)
                {
                    var methodName = method.Identifier.Text;
                    var parameters = method.ParameterList.Parameters;
                    var paramTypes = string.Join(", ", parameters.Select(p => p.Type.ToString()));

                    if (parameters.Count == 0)
                    {
                        sourceBuilder.AppendLine($@"            app.AddSystem({methodName});");
                    }
                    else
                    {
                        sourceBuilder.AppendLine($@"            app.AddSystem<{paramTypes}>({methodName});");
                    }
                }

                sourceBuilder.AppendLine($@"
        }}
    }}
}}
");
                context.AddSource($"{structName}_IPlugin_Inject.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        private static bool IsTargetType(SyntaxNode syntaxNode)
        {
            return syntaxNode is StructDeclarationSyntax structDeclarationSyntax &&
                    structDeclarationSyntax.Members
                        .OfType<MethodDeclarationSyntax>()
                        .Any(m => m.AttributeLists.Count > 0);
        }

        private static StructDeclarationSyntax GetTargetForGeneration(GeneratorSyntaxContext context)
        {
            if (context.Node is StructDeclarationSyntax structDeclarationSyntax)
            {
                foreach (var method in structDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
                {
                    foreach (var attribute in method.AttributeLists.SelectMany(a => a.Attributes))
                    {
                        if (attribute.Name.ToString() == "SystemFunc")
                        {
                            return structDeclarationSyntax;
                        }
                    }
                }
            }
            return null;
        }
    }
}

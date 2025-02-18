#pragma warning disable RS1008 // 不要将每次编译的数据存储到诊断分析器的字段中

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KimerA.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypeConstraintAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticDefine.TypeConstraint_KimerA010);

        private static readonly Dictionary<IMethodSymbol, Dictionary<IParameterSymbol, ImmutableHashSet<ISymbol>>> MethodConstraints = new(SymbolEqualityComparer.Default);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
            context.RegisterSyntaxNodeAction(AnalyzeMethodCall, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is not IMethodSymbol methodSymbol) return;

            // check if the method has parameters
            if (methodSymbol.Parameters.Length is 0) return;

            var parametersConstraints = new Dictionary<IParameterSymbol, ImmutableHashSet<ISymbol>>(SymbolEqualityComparer.Default);

            foreach (var parameter in methodSymbol.Parameters)
            {
                // check if the parameter have TypeConstraintAttribute
                var attrs = parameter.GetAttributes();
                foreach (var attr in attrs)
                {
                    if (attr.AttributeClass?.Name is "TypeConstraintAttribute" && attr.ConstructorArguments.Length is 1)
                    {
                        // Correctly handle the type of ConstructorArguments
                        var allowedTypes = attr.ConstructorArguments[0].Values
                            .Select(arg => arg.Value as INamedTypeSymbol)
                            .ToImmutableHashSet(SymbolEqualityComparer.Default);

                        parametersConstraints[parameter] = allowedTypes;
                    }
                }
            }

            if (parametersConstraints.Any())
            {
                MethodConstraints[methodSymbol] = parametersConstraints;
            }
        }

        private void AnalyzeMethodCall(SyntaxNodeAnalysisContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var invocation = (InvocationExpressionSyntax)context.Node;
            if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol
                || MethodConstraints.ContainsKey(methodSymbol) is false) return;

            var parametersConstraints = MethodConstraints[methodSymbol];

            // 获取这个 method 被调用时的参数
            var arguments = invocation.ArgumentList.Arguments;
            var parameters = methodSymbol.Parameters;

            foreach (var (argument, parameter) in arguments.Zip(parameters, (arg, param) => (arg, param)))
            {
                // 获取参数的类型
                var argumentType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;

                // 检查参数类型是否满足约束条件
                if (parametersConstraints.TryGetValue(parameter, out var allowedTypes))
                {
                    var isValid = allowedTypes.Any(allowed =>
                        SymbolEqualityComparer.Default.Equals(allowed, argumentType) ||
                        argumentType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, allowed)) ||
                        (argumentType.BaseType is not null && SymbolEqualityComparer.Default.Equals(argumentType.BaseType, allowed)));

                    if (isValid is false)
                    {
                        var diagnostic = Diagnostic.Create(DiagnosticDefine.TypeConstraint_KimerA010, argument.GetLocation(), argumentType, string.Join(", ", allowedTypes.Select(type => type.Name)));
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
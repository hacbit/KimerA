#pragma warning disable RS2008 // Enable analyzer release tracking

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KimerA.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ArchiveAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: "KIMERA001",
            title: "Missing Archive<> Field in Archivable Class",
            messageFormat: "Class '{0}' implements IArchivable but does not contain any fields of type Archive<>",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Classes that implement IArchivable should contain at least one field of type Archive<>."
        );

        private static readonly DiagnosticDescriptor Rule2 = new DiagnosticDescriptor(
            id: "KIMERA002",
            title: "Archive<> Field Without IArchivable Implementation",
            messageFormat: "Class '{0}' contains fields of type Archive<> but does not implement IArchivable",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Classes that contain fields of type Archive<> must implement the IArchivable interface."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, Rule2);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = context.Symbol as INamedTypeSymbol;

            // Check if the class implements IArchivable
            var implementsIArchivable = namedTypeSymbol?.AllInterfaces.Any(i => i.Name == "IArchivable");

            // Check if the class contains a field of type Archive<>
            var hasArchiveField = namedTypeSymbol?.GetMembers().OfType<IFieldSymbol>().Any(f =>
                f.Type.Name == "Archive" && f.Type is INamedTypeSymbol namedType && namedType.IsGenericType);
            
            if (hasArchiveField == true && implementsIArchivable == false)
            {
                var diagnostic = Diagnostic.Create(Rule2, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
            else if (hasArchiveField == false && implementsIArchivable == true)
            {
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
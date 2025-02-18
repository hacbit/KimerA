using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KimerA.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ArchiveAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DiagnosticDefine.Archive_KimerA001,
            DiagnosticDefine.Archive_KimerA002,
            DiagnosticDefine.Archive_KimerA003,
            DiagnosticDefine.Archive_KimerA004,
            DiagnosticDefine.Archive_KimerA005,
            DiagnosticDefine.Archive_KimerA006,
            DiagnosticDefine.Archive_KimerA007
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeFieldOrProperty, SyntaxKind.FieldDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is ClassDeclarationSyntax classDeclaration)
            {
                ValueTuple<bool, bool> hasAttribute = (false, false);
                GenericNameSyntax archiveTo = null;

                foreach (var al in classDeclaration.AttributeLists)
                {
                    foreach (var a in al.Attributes)
                    {
                        if (a.Name.ToString() == "ArchiveReceiver")
                        {
                            hasAttribute.Item1 = true;
                        }
                        else if (a.Name is GenericNameSyntax genericName &&
                            genericName.Identifier.Text == "ArchiveTo" &&
                            genericName.TypeArgumentList.Arguments.Count == 1)
                        {
                            hasAttribute.Item2 = true;
                            archiveTo = genericName;
                        }
                    }
                }

                if (hasAttribute is (false, false)) return;

                if (hasAttribute is (true, true))
                {
                    // The class has both ArchiveReceiverAttribute and ArchiveToAttribute
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDefine.Archive_KimerA004, classDeclaration.GetLocation(), classDeclaration.Identifier.Text));
                }
                else if (classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) is false)
                {
                    if (hasAttribute.Item1)
                    {
                        // The class has ArchiveReceiverAttribute but is not partial
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDefine.Archive_KimerA002, classDeclaration.GetLocation(), classDeclaration.Identifier.Text));
                    }
                    else if (hasAttribute.Item2)
                    {
                        // The class has ArchiveToAttribute but is not partial
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDefine.Archive_KimerA003, classDeclaration.GetLocation(), classDeclaration.Identifier.Text));
                    }
                }
                else if (hasAttribute.Item2)
                {
                    var targetType = context.SemanticModel.GetTypeInfo(archiveTo.TypeArgumentList.Arguments.First()).Type;
                    if (targetType is null)
                    {
                        return;
                    }

                    var hasArchiveReceiverAttribute = targetType.GetAttributes()
                        .Any(a => a.AttributeClass?.Name == "ArchiveReceiverAttribute");
                    
                    if (hasArchiveReceiverAttribute is false)
                    {
                        // The type parameter of ArchiveTo<> does not have the ArchiveReceiverAttribute
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDefine.Archive_KimerA001, archiveTo.TypeArgumentList.Arguments.First().GetLocation(), targetType.Name));
                    }
                }
            }
        }

        private void AnalyzeFieldOrProperty(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is MemberDeclarationSyntax memberDeclaration)
            {
                // check if the member has Archivable attribute
                var hasArchivableAttributes = memberDeclaration.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(a => a.Name.ToString() == "Archivable");

                if (hasArchivableAttributes)
                {
                    if (memberDeclaration.Parent is not ClassDeclarationSyntax parentClass)
                    {
                        return;
                    }

                    var archiveTo = parentClass.AttributeLists.SelectMany(al => al.Attributes)
                        .FirstOrDefault(a => a.Name is GenericNameSyntax genericName &&
                            genericName.Identifier.Text == "ArchiveTo" &&
                            genericName.TypeArgumentList.Arguments.Count == 1);
                    
                    if (archiveTo is null)
                    {
                        var ident = memberDeclaration is FieldDeclarationSyntax fieldDeclaration
                            ? fieldDeclaration.Declaration.Variables.First().Identifier
                            : (memberDeclaration as PropertyDeclarationSyntax)?.Identifier;
                        // The member has ArchivableAttribute but the class does not have ArchiveToAttribute
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDefine.Archive_KimerA005, memberDeclaration.GetLocation(), ident));
                    }

                    if (memberDeclaration is FieldDeclarationSyntax fieldSyntax)
                    {
                        var isReadOnly = fieldSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
                        if (isReadOnly)
                        {
                            // The member has ArchivableAttribute but is readonly
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDefine.Archive_KimerA006, memberDeclaration.GetLocation(), fieldSyntax.Declaration.Variables.First().Identifier));
                        }
                    }
                    else if (memberDeclaration is PropertyDeclarationSyntax propertySyntax)
                    {
                        var hasSetAccessor = propertySyntax.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false;
                        if (hasSetAccessor is false)
                        {
                            // The member has ArchivableAttribute but does not have a set accessor
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDefine.Archive_KimerA007, memberDeclaration.GetLocation(), propertySyntax.Identifier));
                        }
                    }
                }
            }
        }
    }
}
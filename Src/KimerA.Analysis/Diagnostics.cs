#pragma warning disable RS2008 // Enable analyzer release tracking

using System.Globalization;
using System.Resources;
using Microsoft.CodeAnalysis;

namespace KimerA.Analysis
{
    /// <summary>
    /// Provides all the diagnostics used in the KimerA.Analysis namespace.
    /// </summary>
    public static class DiagnosticDefine
    {
        private static readonly ResourceManager resourceManager = new("KimerA.Analysis.Resources", typeof(DiagnosticDefine).Assembly);

        private static string GetResourceString(string name) => resourceManager.GetString(name, CultureInfo.CurrentCulture) ?? name;

        /// <summary>
        /// ArchiveTo Target Without ArchiveReceiverAttribute
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA001 = new DiagnosticDescriptor(
            id: "KIMERA001",
            title: GetResourceString("Archive_KimerA001_Title"),
            messageFormat: GetResourceString("Archive_KimerA001_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA001_Description")
        );

        /// <summary>
        /// The Class Has ArchiveReceiverAttribute Is Not Partial
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA002 = new DiagnosticDescriptor(
            id: "KIMERA002",
            title: GetResourceString("Archive_KimerA002_Title"),
            messageFormat: GetResourceString("Archive_KimerA002_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA002_Description")
        );

        /// <summary>
        /// The Class Has ArchiveToAttribute Is Not Partial
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA003 = new DiagnosticDescriptor(
            id: "KIMERA003",
            title: GetResourceString("Archive_KimerA003_Title"),
            messageFormat: GetResourceString("Archive_KimerA003_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA003_Description")
        );

        /// <summary>
        /// The Class Cannot Have Both ArchiveToAttribute And ArchiveReceiverAttribute
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA004 = new DiagnosticDescriptor(
            id: "KIMERA004",
            title: GetResourceString("Archive_KimerA004_Title"),
            messageFormat: GetResourceString("Archive_KimerA004_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA004_Description")
        );

        /// <summary>
        /// The member has the ArchivableAttribute but is not in a class with the ArchiveToAttribute
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA005 = new DiagnosticDescriptor(
            id: "KIMERA005",
            title: GetResourceString("Archive_KimerA005_Title"),
            messageFormat: GetResourceString("Archive_KimerA005_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA005_Description")
        );

        /// <summary>
        /// The field with ArchivableAttribute cannot be readonly
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA006 = new DiagnosticDescriptor(
            id: "KIMERA006",
            title: GetResourceString("Archive_KimerA006_Title"),
            messageFormat: GetResourceString("Archive_KimerA006_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA006_Description")
        );

        /// <summary>
        /// The property with ArchivableAttribute must have a setter
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA007 = new DiagnosticDescriptor(
            id: "KIMERA007",
            title: GetResourceString("Archive_KimerA007_Title"),
            messageFormat: GetResourceString("Archive_KimerA007_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA007_Description")
        );

        /// <summary>
        /// Type specified archive receiver but not registered
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA008 = new DiagnosticDescriptor(
            id: "KIMERA008",
            title: GetResourceString("Archive_KimerA008_Title"),
            messageFormat: GetResourceString("Archive_KimerA008_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA008_Description")
        );

        /// <summary>
        /// Type specified archive receiver but registered to another receiver
        /// </summary>
        public static readonly DiagnosticDescriptor Archive_KimerA009 = new DiagnosticDescriptor(
            id: "KIMERA009",
            title: GetResourceString("Archive_KimerA009_Title"),
            messageFormat: GetResourceString("Archive_KimerA009_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetResourceString("Archive_KimerA009_Description")
        );

        /// <summary>
        /// Invalid Argument Type
        /// </summary>
        public static readonly DiagnosticDescriptor TypeConstraint_KimerA010 = new DiagnosticDescriptor(
            id: "KIMERA010",
            title: GetResourceString("TypeConstraint_KimerA010_Title"),
            messageFormat: GetResourceString("TypeConstraint_KimerA010_MessageFormat"),
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: GetResourceString("TypeConstraint_KimerA010_Description")
        );
    }
}
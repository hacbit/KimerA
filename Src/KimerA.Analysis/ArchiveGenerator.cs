using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace KimerA.Analysis
{
    [Generator(LanguageNames.CSharp)]
    public sealed class ArchiveGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var archiveProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => s is ClassDeclarationSyntax decl && decl.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name is GenericNameSyntax genericName && genericName.Identifier.Text == "ArchiveTo" && genericName.TypeArgumentList.Arguments.Count == 1),
                    transform: (ctx, _) => ctx.Node as ClassDeclarationSyntax
                ).Where(m => m is not null)
                .Collect();
            
            var receiverProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => s is ClassDeclarationSyntax decl && decl.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString() == "ArchiveReceiver"),
                    transform: (ctx, _) => ctx.Node as ClassDeclarationSyntax
                ).Where(m => m is not null)
                .Collect();

            context.RegisterSourceOutput(archiveProvider.Combine(receiverProvider), (spc, source) =>
            {
                var (archives, receivers) = source;
                foreach (var @class in archives)
                {
                    // get have [Archivable] fields or properties
                    var members = @class.Members
                        .Where(
                            m => m is FieldDeclarationSyntax or PropertyDeclarationSyntax &&
                            m.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString() == "Archivable")
                        ).ToImmutableHashSet();
                    ImplementArchiveTo(spc, @class, members);
                }

                var map = GetReceiverToArchiveMap(archives, receivers);
                foreach (var pair in map)
                {
                    ImplementArchiveReceiver(spc, pair.Key, pair.Value);
                }
            });
        }

        private Dictionary<ClassDeclarationSyntax, IEnumerable<ClassDeclarationSyntax>> GetReceiverToArchiveMap(IEnumerable<ClassDeclarationSyntax> archives, IEnumerable<ClassDeclarationSyntax> receivers)
        {
            var map = new Dictionary<ClassDeclarationSyntax, IEnumerable<ClassDeclarationSyntax>>();
            foreach (var archive in archives)
            {
                var archiveTo = archive.AttributeLists.SelectMany(al => al.Attributes).First(a => a.Name is GenericNameSyntax genericName && genericName.Identifier.Text == "ArchiveTo" && genericName.TypeArgumentList.Arguments.Count == 1);
                var archiveToType = (archiveTo.Name as GenericNameSyntax).TypeArgumentList.Arguments.First().ToString();
                var receiver = receivers.FirstOrDefault(r => r.Identifier.Text == archiveToType);
                if (receiver is not null)
                {
                    if (map.ContainsKey(receiver))
                    {
                        map[receiver] = map[receiver].Append(archive);
                    }
                    else
                    {
                        map[receiver] = new[] { archive };
                    }
                }
            }
            return map;
        }

        private void ImplementArchiveReceiver(SourceProductionContext context, ClassDeclarationSyntax receiver, IEnumerable<ClassDeclarationSyntax> classes)
        {
            var ns = GetNamespaceName(receiver);
            var nsStr = $"namespace {ns} {{\n";
            var receiverName = receiver.Identifier.Text;

            var sha256Name = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(GetFullName(receiver))).Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("x2")), sb => sb.ToString());

            var classIdents = classes.Select(c => c.Identifier.Text);

            var impl = $@"// <auto-generated>

using System;
using System.Collections.Generic;

{(string.IsNullOrEmpty(ns) ? string.Empty : nsStr)}
partial class {receiverName}
{{
    public static {receiverName} Instance {{ get; }} = new();

    private static Dictionary<Type, KimerA.IArchiveSender> m_Senders = new();

    public const string ArchiveName = ""{sha256Name}"";

    private {receiverName}() {{ }}

    /// <summary>
    /// Called before serialization.
    /// </summary>
    partial void OnSaveBegin();

    /// <summary>
    /// Called after writing to the archive file.
    /// </summary>
    partial void OnSaveEnd();

    /// <summary>
    /// Called before reading from the archive file and deserialization.
    /// </summary>
    partial void OnLoadBegin();

    /// <summary>
    /// Called after all registered senders have been loaded.
    /// </summary>
    partial void OnLoadEnd();

    /// <summary>
    /// Called after the data is serialized and before writing to the archive file.
    /// <para>You can partial implement this method to encrypt the serialized data.</para>
    /// </summary>
    partial void OnProcessSerialize(ref string data);

    /// <summary>
    /// Called after reading from the archive file and before deserialization.
    /// <para>You can partial implement this method to decrypt the serialized data.</para>
    /// </summary>
    partial void OnProcessDeserialize(ref string data);

    /// <summary>
    /// Register the sender to the receiver, return false if the sender has been registered.
    /// Each type of sender can only be registered once.
    /// </summary>
    public bool TryRegister<TSender>(TSender sender) where TSender : KimerA.IArchiveSender
    {{
        if (m_Senders.ContainsKey(typeof(TSender))) return false;
        m_Senders.Add(typeof(TSender), sender);
        return true;
    }}

    /// <summary>
    /// Save all registered senders to the archive file.
    /// </summary>
    public void Save()
    {{
        OnSaveBegin();
        var archive = new Dictionary<string, object>();
        foreach (var sender in m_Senders)
        {{
            archive.Add(sender.Key.FullName, sender.Value.Save());
        }}
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(archive);
        OnProcessSerialize(ref json);
        KimerA.FileSystem.WriteText(ArchiveName, json);
        OnSaveEnd();
    }}

    /// <summary>
    /// Load all registered senders from the archive file.
    /// </summary>
    public void Load()
    {{
        OnLoadBegin();
        var json = KimerA.FileSystem.ReadText(ArchiveName);
        OnProcessDeserialize(ref json);
        var archive = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        foreach (var sender in m_Senders)
        {{
            if (archive.TryGetValue(sender.Key.FullName, out var data))
            {{
                sender.Value.Load(data.ToString());
            }}
        }}
        OnLoadEnd();
    }}
}}
{(string.IsNullOrEmpty(ns) ? string.Empty : "}")}
";
            context.AddSource($"{receiverName}.ArchiveReceiver.g.cs", SourceText.From(impl, Encoding.UTF8));
        }

        private void ImplementArchiveTo(SourceProductionContext context, ClassDeclarationSyntax @class, ImmutableHashSet<MemberDeclarationSyntax> members)
        {
            var ns = GetNamespaceName(@class);
            var nsStr = $"namespace {ns} {{\n";
            var className = @class.Identifier.Text;

            // get members type and name
            // likes: int Age; string Name;
            var membersTypes = members.Select(m =>
            {
                var type = m switch
                {
                    FieldDeclarationSyntax field => field.Declaration.Type,
                    PropertyDeclarationSyntax property => property.Type,
                    _ => null
                };
                return type?.ToString();
            }).Where(t => t is not null);

            var membersIdents = members.Select(m =>
            {
                var ident = m switch
                {
                    FieldDeclarationSyntax field => field.Declaration.Variables.First().Identifier.Text,
                    PropertyDeclarationSyntax property => property.Identifier.Text,
                    _ => null
                };
                return ident;
            }).Where(i => i is not null);

            var impl = $@"// <auto-generated>

using System;
using System.Collections.Generic;

{(string.IsNullOrEmpty(ns) ? string.Empty : nsStr)}
partial class {className} : KimerA.IArchiveSender
{{
    string KimerA.IArchiveSender.Save()
    {{
        return Newtonsoft.Json.JsonConvert.SerializeObject(new
        {{
            {string.Join(",\n            ", membersIdents)}
        }});
    }}

    void KimerA.IArchiveSender.Load(string value)
    {{
        var data = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(value, new
        {{
            {string.Join(",\n            ", membersIdents.Zip(membersTypes, (i, t) => $"{i} = default({t})"))}
        }});
        if (data is not null)
        {{
            {string.Join("\n            ", membersIdents.Select(m => $"{m} = data.{m};"))}
        }}
    }}
}}
{(string.IsNullOrEmpty(ns) ? string.Empty : "}")}
";
            context.AddSource($"{className}.ArchiveSender.g.cs", SourceText.From(impl, Encoding.UTF8));
        }

        private string GetNamespaceName(ClassDeclarationSyntax classDeclaration)
        {
            var ns = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return ns is null ? string.Empty : ns.Name.ToString();
        }

        private string GetFullName(ClassDeclarationSyntax classDeclaration)
        {
            var ns = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return ns is null ? classDeclaration.Identifier.Text : $"{ns.Name}.{classDeclaration.Identifier.Text}";
        }
    }
}
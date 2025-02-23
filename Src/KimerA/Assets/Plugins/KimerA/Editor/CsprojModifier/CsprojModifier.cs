#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace KimerA.Editor.CsprojModifier
{
    public sealed class CsprojModifier : EditorWindow
    {
        [MenuItem("KimerA/Csproj Modifier")]
        public static void ShowWindow()
        {
            var window = GetWindow<CsprojModifier>("Csproj Modifier");
            window.Show();
        }

        private void OnProjectChange()
        {
            LoadConfigs();
            ChangeCsprojs();
        }

        private void OnGUI()
        {
            // draw additional configs
            EditorGUILayout.LabelField("Additional Configs");
            for (var i = 0; i < additionalConfigs.Count; i++)
            {
                var path = additionalConfigs[i];
                // draw a label and a button to remove the config
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(path);
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Remove"))
                {
                    additionalConfigs.Remove(path);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Config"))
            {
                AddAdditionalConfig();
            }
            if (GUILayout.Button("Regenerate And Modify Csproj Files"))
            {
                if (CodeEditor.CurrentEditor.GetType().Name == "DefaultExternalCodeEditor")
                {
                    // SyncVS.Synchronizer.Sync(); (SyncVS is an internal class, so call it with Reflection)

                    var syncVsType = Type.GetType("UnityEditor.SyncVS, UnityEditor");
                    ThrowIfNull(syncVsType, "Type 'UnityEditor.SyncVS' is not found on the editor.");

                    var slnSynchronizerType = Type.GetType("UnityEditor.VisualStudioIntegration.SolutionSynchronizer, UnityEditor");
                    ThrowIfNull(slnSynchronizerType, "Type 'UnityEditor.VisualStudioIntegration.SolutionSynchronizer' is not found on the editor.");

                    var solutionSynchronizerField = syncVsType.GetField("Synchronizer", BindingFlags.Static | BindingFlags.NonPublic);
                    ThrowIfNull(solutionSynchronizerField, "Field 'Synchronizer' is not found in 'SolutionSynchronizer'.");

                    var syncMethod = slnSynchronizerType.GetMethod("Sync", BindingFlags.Instance | BindingFlags.Public);
                    ThrowIfNull(syncMethod, "Method 'Sync' is not found in 'Synchronizer'.");

                    syncMethod.Invoke(solutionSynchronizerField.GetValue(null), Array.Empty<object>());
                }
                else
                {
                    // HACK: Make it look like a dummy file has been added.
                    CodeEditor.CurrentEditor.SyncIfNeeded(new [] { "RegenerateProjectFeature.cs" }, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
                }

                ChangeCsprojs();
            }
            if (GUILayout.Button("Modify Csproj Files"))
            {
                ChangeCsprojs();
            }
        }

        private void ThrowIfNull(object value, string msg)
        {
            if (value == null)
            {
                throw new Exception(msg);
            }
        }

        private static readonly List<string> additionalConfigs = new();

        private static void AddAdditionalConfig()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Select a config file", Application.dataPath, new[] {"Props", "props"});
            if (!string.IsNullOrEmpty(path))
            {
                additionalConfigs.Add(path);
                LoadConfigs();
            }
        }

        public const string DefaultConfigPath = "Assets/Plugins/KimerA/Editor/CsprojModifier/CsprojModifierConfig.props";

        private static Dictionary<string, string> m_Properties = new();

        public static void LoadConfigs()
        {
            var properties = new Dictionary<string, string>();
            foreach (var path in new[] {DefaultConfigPath}.Concat(additionalConfigs))
            {
                if (File.Exists(path))
                {
                    var xdoc = XDocument.Load(path);
                    foreach (var e in xdoc.Descendants().Where(e => e.Name.LocalName == "PropertyGroup").Descendants())
                    {
                        properties[e.Name.LocalName] = e.Value;
                    }
                }
            }
            m_Properties = properties;
        }

        [InitializeOnLoadMethod]
        public static void Init()
        {
            CompilationPipeline.compilationFinished += _ =>
            {
                LoadConfigs();
                ChangeCsprojs();
                Debug.Log("CsprojModifier: Csproj files have been modified.");
            };
        }

        private static void ChangeCsprojs()
        {
            // csproj is the FullPath of the .csproj file
            foreach (var csproj in GetFilteredCsprojFiles())
            {
                var xdoc = XDocument.Load(csproj);
                var properties = xdoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "LangVersion")?.Parent!;

                // Clone for local use
                var localProperties = m_Properties.ToDictionary(e => e.Key, e => e.Value);

                // find .asmdef directory, and check if has csc.rsp file
                var asmdefName = Path.GetFileNameWithoutExtension(csproj);
                var asmdefPath = AssetDatabase.FindAssets($"t:asmdef {asmdefName}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault();
                if (asmdefPath != null)
                {
                    var asmdefDir = Path.GetDirectoryName(asmdefPath);
                    var cscRspPath = Path.Combine(asmdefDir, "csc.rsp");
                    if (File.Exists(cscRspPath))
                    {
                        var cscRsp = File.ReadAllText(cscRspPath);
                        var matches = Regex.Matches(cscRsp, @"-(?<key>\w+):?(?<value>[^ ]*)");

                        foreach (Match match in matches)
                        {
                            var key = match.Groups["key"].Value;
                            var value = match.Groups["value"].Value;

                            switch (key)
                            {
                                case "langversion":
                                    localProperties["LangVersion"] = value;
                                    break;
                                case "define":
                                    var symbols = value.Split(';');
                                    foreach (var symbol in symbols)
                                    {
                                        localProperties["DefineConstants"] = localProperties.TryGetValue("DefineConstants", out var existingValue)
                                            ? existingValue + ";" + symbol
                                            : symbol;
                                    }
                                    break;
                                case "nowarn":
                                    var warnings = value.Split(';');
                                    foreach (var warning in warnings)
                                    {
                                        localProperties["NoWarn"] = localProperties.TryGetValue("NoWarn", out var existingValue)
                                            ? existingValue + ";" + warning
                                            : warning;
                                    }
                                    break;
                                case "nullable":
                                    localProperties["Nullable"] = "enable";
                                    break;
                                default:
                                    // Not supported yet, ignore it
                                    // localProperties[key] = value;
                                    break;
                            }
                        }
                    }
                }

                foreach (var (key, value) in localProperties)
                {
                    ChangeElementOrAdd(properties, key, value);
                }
                xdoc.Save(csproj);
            }
        }

        private static void ChangeElementOrAdd(XElement properties, string elementName, string value)
        {
            var element = properties.Descendants().FirstOrDefault(e => e.Name.LocalName == elementName);
            if (element != null)
            {
                element.Value = value;
            }
            else
            {
                properties.Add(new XElement(elementName, value));
            }
        }

        private static IEnumerable<string> GetFilteredCsprojFiles()
        {
            var json = File.ReadAllText(Path.Combine(Directory.GetParent(DefaultConfigPath).FullName, "CsprojsToChange.json"));
            var csprojs = JsonUtility.FromJson<CsprojsToChange>(json);

            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var allCsprojs = Directory.GetFiles(projectRoot, "*.csproj", SearchOption.TopDirectoryOnly);

            var filteredCsprojFiles = new List<string>();
            foreach (var pattern in csprojs.Csprojs)
            {
                var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                filteredCsprojFiles.AddRange(allCsprojs.Where(csproj => regex.IsMatch(Path.GetFileName(csproj))));
            }

            return filteredCsprojFiles.Distinct();
        }

        [Serializable]
        private class CsprojsToChange
        {
            public required string[] Csprojs;
        }
    }

    [InitializeOnLoad]
    public static class AutoRegen
    {
        static AutoRegen()
        {
            CsprojModifier.Init();
        }
    }
}

#endif
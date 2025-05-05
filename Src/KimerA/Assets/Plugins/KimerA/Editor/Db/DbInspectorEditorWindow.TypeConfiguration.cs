#if UNITY_EDITOR && ODIN_INSPECTOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using KimerA.Utils;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace KimerA.Db
{
    public sealed partial class DbInspectorEditorWindow
    {
        public sealed class TypeConfiguration
        {
            public TypeConfiguration()
            {
                LoadTypeConfig();
            }

            [ShowInInspector, ReadOnly]
            [BoxGroup("Allowed C# Types")]
            public static readonly List<string> AllowedDefaultCsTypes = TypeUtil.DefaultTypes.ToList();

            [ShowInInspector, ReadOnly]
            [BoxGroup("Allowed C# Types")]
            public static readonly List<string> AllowedAdditionalCsTypes = new();

            [Button("Add New Allowed CsType", Expanded = true, Style = ButtonStyle.Box)]
            private void AddNewAllowedCsType([ValueDropdown(nameof(GetAllTypes))] string type)
            {
                if (type == null)
                {
                    EditorUtility.DisplayDialog("Warning", "Please select a type", "OK");
                    return;
                }
                if (AllowedDefaultCsTypes.Contains(type) || AllowedAdditionalCsTypes.Contains(type))
                {
                    EditorUtility.DisplayDialog("Warning", "This type is already exist", "OK");
                    return;
                }
                AllowedAdditionalCsTypes.Add(type);
            }

            [Button("Remove Allowed CsType", Expanded = true, Style = ButtonStyle.Box)]
            private void RemoveAllowedCsType([ValueDropdown("AllowedAdditionalCsTypes")] string type)
            {
                AllowedAdditionalCsTypes.Remove(type);
            }

            private static IEnumerable<string> GetAllTypes()
            {
                return TypeUtil.AllTypes.Select(type => type.FullName);
            }

            public static readonly string TypeConfigFilePath = Path.Combine(ConfigDirPath, "TypeConfig.json");

            [PropertySpace(10, 10)]
            [ButtonGroup("Type Config")]
            [Button("Load Type Config")]
            private void LoadTypeConfig()
            {
                if (File.Exists(TypeConfigFilePath) == false)
                {
                    Debug.LogWarning("TypeConfig.json is not exist");
                    return;
                }

                var json = File.ReadAllText(TypeConfigFilePath);
                AllowedAdditionalCsTypes.Clear();
                AllowedAdditionalCsTypes.AddRange(JsonConvert.DeserializeObject<List<string>>(json));

                Debug.Log($"Load Type Config from {TypeConfigFilePath}");
            }

            [PropertySpace(10, 10)]
            [ButtonGroup("Type Config")]
            [Button("Save Type Config")]
            private void SaveTypeConfig()
            {
                var dir = Path.GetDirectoryName(TypeConfigFilePath);
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonConvert.SerializeObject(AllowedAdditionalCsTypes);
                File.WriteAllText(TypeConfigFilePath, json);

                Debug.Log($"Save Type Config to {TypeConfigFilePath}");
            }

            [PropertySpace(10, 10)]
            [ButtonGroup("Type Config")]
            [Button("Clear Type Config")]
            private void ClearTypeConfig()
            {
                File.Delete(TypeConfigFilePath);
                File.Delete($"{TypeConfigFilePath}.meta");
                Debug.Log("Clear Type Config Success");
            }
        }
    }
}

#endif
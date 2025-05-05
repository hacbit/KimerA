#if UNITY_EDITOR && ODIN_INSPECTOR

namespace KimerA.Editor
{
    using UnityEditor;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector;
    using KimerA.Utils;
    using System.IO;
    using UnityEngine;

    public sealed class SettingEditorWindow : OdinEditorWindow
    {
        [MenuItem("KimerA/Setting Editor")]
        public static void ShowWindow()
        {
            GetWindow<SettingEditorWindow>("Setting Editor").Show();
        }

        [ShowInInspector]
        public const string SettingFilePath = "Assets/Settings";

        public SettingConfig SettingConfig = new();

        [ValueDropdown("@SettingConfig.GetFileNames()")]
        [ShowInInspector]
        public string CurrentSettingFile
        {
            get => SettingConfig.CurrentSettingFile!;
            set => SettingConfig.CurrentSettingFile = value;
        }

        public static readonly string SettingConfigSavePath = Path.Combine(SettingFilePath, "SettingConfig.asset");

        protected override void OnEnable()
        {
            base.OnEnable();
            SettingConfig = ResUtil.GetAsset<SettingConfig>() ?? InitSettingFile();
        }

        public static SettingConfig InitSettingFile()
        {
            var config = ResUtil.GetOrCreateAsset<SettingConfig>(SettingConfigSavePath);
            ResUtil.AddAssetToGroup("SeetingConfig", SettingConfigSavePath, true);
            config.CurrentSettingFile = "Default";
            GenerateNewSettingFile();
            return config;
        }

        [Button]
        public static void GenerateNewSettingFile(string fileName = "Default")
        {
            var targetPath = Path.Combine(SettingFilePath, $"{fileName}.json");
            DirectoryUtil.ApplyDirectory(SettingFilePath);

            if (File.Exists(targetPath))
            {
                EditorUtility.DisplayDialog("Error", "File already exists", "OK");
                return;
            }

            File.WriteAllText(targetPath, "{}");
            AssetDatabase.ImportAsset(targetPath);
            
            RefreshSettingFile(ResUtil.GetAsset<SettingConfig>()!);
        }

        public static void RefreshSettingFile(SettingConfig config)
        {
            config.SettingFiles.Clear();
            config.SettingFiles.AddRange(ResUtil.GetAssetsInFolder<TextAsset>(SettingFilePath));
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

#endif
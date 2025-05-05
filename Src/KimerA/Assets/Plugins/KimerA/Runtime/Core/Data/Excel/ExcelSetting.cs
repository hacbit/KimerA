#if ODIN_INSPECTOR

namespace KimerA.Data.Excel
{
    using System;
    using System.Collections.Generic;
    using KimerA.Data.Excel.Config;
    using Sirenix.OdinInspector;
    using UnityEditor;

    [Serializable]
    public sealed class ExcelSetting
    {
        public const string DefaultExcelDataPathRoot = "Assets/Excel/RowData";

        [ValueDropdown("@KimerA.Utils.DirectoryUtil.GetFilePaths()")]
        public string ExcelDataPathRoot = DefaultExcelDataPathRoot;

        public const string DefaultAssetDataPathRoot = "Assets/Excel/AssetData";

        [ValueDropdown("@KimerA.Utils.DirectoryUtil.GetFilePaths()")]
        public string AssetDataPathRoot = DefaultAssetDataPathRoot;

        public const string DefaultGeneratedScriptPathRoot = "Assets/Scripts/Generated/Excel";

        [ValueDropdown("@KimerA.Utils.DirectoryUtil.GetFilePaths()")]
        public string GeneratedScriptPathRoot = DefaultGeneratedScriptPathRoot;

        public const string DefaultGeneratedCodeNamespace = "KimerA.Generate";

        public string GeneratedCodeNamespace = DefaultGeneratedCodeNamespace;

        public List<ExcelStructConfig> StructConfigs = new();

        [Button("Reset Setting", ButtonSizes.Medium)]
        private void Reset()
        {
            if (EditorUtility.DisplayDialog("Reset Setting", "Are you sure you want to reset the setting?", "Yes", "No"))
            {
                ExcelDataPathRoot = DefaultExcelDataPathRoot;
                AssetDataPathRoot = DefaultAssetDataPathRoot;
                GeneratedScriptPathRoot = DefaultGeneratedScriptPathRoot;
                GeneratedCodeNamespace = DefaultGeneratedCodeNamespace;
                StructConfigs.Clear();
            }
        }
    }
}

#endif
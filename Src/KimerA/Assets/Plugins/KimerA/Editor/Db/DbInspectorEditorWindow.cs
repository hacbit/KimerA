#if UNITY_EDITOR

using System.IO;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace KimerA.Db
{
    public sealed partial class DbInspectorEditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("KimerA/Db Inspector")]
        public static void ShowWindow()
        {
            var window = GetWindow<DbInspectorEditorWindow>("Db Inspector");
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree
            {
                { "Common", new Common() },
                { "Type Configuration", new TypeConfiguration() },
                { "Table Manager", new TableManager() },
                { "Table Viewer", new TableViewer() },
                { "Db Command", new DbCommand() }
            };

            return tree;
        }

        public static readonly string ConfigDirPath = Path.Combine(Application.dataPath, "KimerAConfig", "Db");

        public string DbPath => DbControl.DbPath;
    }
}

#endif
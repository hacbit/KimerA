#if UNITY_EDITOR && ODIN_INSPECTOR

using Sirenix.OdinInspector;
using UnityEditor;

namespace KimerA.Db
{
    public sealed partial class DbInspectorEditorWindow
    {
        public sealed class Common
        {
            [Button("Nothing Here", Expanded = true, Style = ButtonStyle.Box)]
            private void PopWarning()
            {
                EditorUtility.DisplayDialog("Warning", "Not implemented yet", "OK");
            }
        }
    }
}

#endif
#if UNITY_EDITOR && ODIN_INSPECTOR

using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace KimerA.Editor
{
    public sealed class CommonEditorWindow : OdinEditorWindow
    {
        [MenuItem("KimerA/Common")]
        public static void ShowWindow()
        {
            var window = GetWindow<CommonEditorWindow>("Common");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
    }
}

#endif
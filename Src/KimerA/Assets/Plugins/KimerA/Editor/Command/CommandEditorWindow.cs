#if UNITY_EDITOR

using Sirenix.OdinInspector.Editor;
using UnityEditor;

public sealed class CommandEditorWindow : OdinEditorWindow
{
    [MenuItem("KimerA/Command Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<CommandEditorWindow>();
        window.Show();
    }
}

#endif
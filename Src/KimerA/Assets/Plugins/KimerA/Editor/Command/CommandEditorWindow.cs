#if UNITY_EDITOR

using UnityEditor;

public sealed class CommandEditorWindow : EditorWindow
{
    [MenuItem("KimerA/Command Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<CommandEditorWindow>();
        window.Show();
    }
}

#endif
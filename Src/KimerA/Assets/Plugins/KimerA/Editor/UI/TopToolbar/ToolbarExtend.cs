#if UNITY_EDITOR

using KimerA.Editor.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KimerA.Editor.UI;

[InitializeOnLoad]
internal static partial class ToolbarExtend
{
    static ToolbarExtend()
    {
        if (IsRegisterDefaulttOnToolbar)
        {
            RegisterToToolbar();
        }
        else
        {
            UnregisterFromToolbar();
        }
    }

    private const string k_Storage_RegisterDefaultOnToolbar = "UI/RegisterDefaultOnToolbar";

    public static bool IsRegisterDefaulttOnToolbar
    {
        get => EditorStorageUtil.GetValueOrDefault(k_Storage_RegisterDefaultOnToolbar, true);
        set => EditorStorageUtil.SetValue(k_Storage_RegisterDefaultOnToolbar, value);
    }

    private static void RegisterToToolbar()
    {
        UnregisterFromToolbar();
        ToolbarHook.OnToolbarGUIRight += OnGUI;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void UnregisterFromToolbar()
    {
        ToolbarHook.OnToolbarGUIRight -= OnGUI;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private static void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button(new GUIContent("Fast Play", "直接从主场景开始运行游戏")))
        {
            StartFromFirstScene();
        }
        GUILayout.EndHorizontal();
    }

    private static string prevScenePath;
    private static bool isQuickStart = false;

    private static void StartFromFirstScene()
    {
        if (EditorBuildSettings.scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("加载失败", "Build Settings 中没有场景", "好的");
            return;
        }

        var firstPath = EditorBuildSettings.scenes[0].path;
        var activeScene = EditorSceneManager.GetActiveScene();

        if (activeScene.path == firstPath)
        {
            EditorApplication.isPlaying = true;
            return;
        }

        // 记录当前场景信息
        prevScenePath = activeScene.path;
        isQuickStart = true;

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorSceneManager.OpenScene(firstPath, OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state is PlayModeStateChange.EnteredEditMode && isQuickStart)
        {
            isQuickStart = false;
            if (string.IsNullOrEmpty(prevScenePath) is false)
            {
                EditorSceneManager.OpenScene(prevScenePath, OpenSceneMode.Single);
            }
        }
    }
}

#endif
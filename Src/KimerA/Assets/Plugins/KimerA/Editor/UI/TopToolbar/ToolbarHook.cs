#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace KimerA.Editor.UI;

public static class ToolbarHook
{
    public static readonly Type Toolbar_Type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
    public static readonly Type GUIView_Type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
#if UNITY_2020_1_OR_NEWER
    public static readonly Type IWindowBackend_Type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.IWindowBackend");
    public static readonly PropertyInfo windowBackend = GUIView_Type.GetProperty("windowBackend", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    public static readonly PropertyInfo visualTree = IWindowBackend_Type.GetProperty("visualTree", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#else
    public static PropertyInfo visualTree = GUIView_Type.GetProperty("visualTree", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
#endif
    public static FieldInfo m_OnGUIHandler = typeof(IMGUIContainer).GetField("m_OnGUIHandler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    public static ScriptableObject CurrentToolbar;

#if UNITY_2021_1_OR_NEWER
    public static event Action OnToolbarGUILeft;
    public static event Action OnToolbarGUIRight;
#else
    public static event Action OnToolbarGUI;
#endif

    static ToolbarHook()
    {
        EditorApplication.update -= OnUpdate;
        EditorApplication.update += OnUpdate;
    }

    private static void OnUpdate()
    {
        if (CurrentToolbar != null) return;

        // find toolbar
        var toolbars = Resources.FindObjectsOfTypeAll(Toolbar_Type);
        CurrentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;

        if (CurrentToolbar != null)
        {
#if UNITY_2021_1_OR_NEWER
            var root = CurrentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            var rawRoot = root.GetValue(CurrentToolbar);
            var mRoot = rawRoot as VisualElement;
            RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUILeft);
            RegisterCallback("ToolbarZoneRightAlign", OnToolbarGUIRight);

            void RegisterCallback(string root, Action action)
            {
                var toolbarZone = mRoot.Q(root);
                var parent = new VisualElement()
                {
                    style = { flexGrow = 1, flexDirection = FlexDirection.Row }
                };
                var container = new IMGUIContainer();
                container.onGUIHandler += action;
                parent.Add(container);
                toolbarZone.Add(parent);
            }
#else
#if UNITY_2020_1_OR_NEWER
            var windowBackend = windowBackend_Prop.GetValue(CurrentToolbar);

            // Get it's visual tree
            var _visualTree = (VisualElement)visualTree.GetValue(windowBackend, null);
#else
            // Get it's visual tree
            var _visualTree = (VisualElement)visualTree.GetValue(CurrentToolbar, null);
#endif
            // Get first child which 'happens' to be toolbar IMGUIContainer
            var container = (IMGUIContainer)_visualTree[0];

            var handler = (Action)m_OnGUIHandler.GetValue(container);
            handler -= OnToolbarGUI;
            handler += OnToolbarGUI;
            m_OnGUIHandler.SetValue(container, handler);
#endif
        }
    }
}

#endif
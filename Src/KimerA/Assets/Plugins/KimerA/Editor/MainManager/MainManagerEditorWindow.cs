#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using KimerA.Editor.UI;
using KimerA.Editor.Utils;
using UnityEngine.UIElements;

namespace KimerA.Editor;

/// <summary>
/// Editor window all in one
/// </summary>
public sealed class MainManagerEditorWindow : KimeraMenuEditorWindow
{
    private static MainManagerEditorWindow m_Instance;

    public static MainManagerEditorWindow Instance
    {
        get
        {
            m_Instance ??= CreateInstance<MainManagerEditorWindow>();
            return m_Instance;
        }
        private set => m_Instance = value;
    }

    [MenuItem("KimerA/Main Manager")]
    public static void Open()
    {
        var window = GetWindow<MainManagerEditorWindow>();
        window.titleContent = new GUIContent("Main Manager");
        Instance = window;
        window.Show();
    }

    protected override TreeViewItemData<KimeraMenuItemData> OnMenuInit()
    {
        var tree = new TreeViewItemData<KimeraMenuItemData>("Main Manager Root".GetHashCode(), new KimeraMenuItemData("Root", null));
        CollectMainManagerItems(tree);
        return tree;
    }

    private static class CacheHelper
    {
        static CacheHelper()
        {
            TypeUtil.QueryTypesWithAttribute<MainManagerItemAttribute>();
        }
    }

    private static void CollectMainManagerItems(TreeViewItemData<KimeraMenuItemData> tree)
    {
        var types = TypeUtil.QueryTypesWithAttribute<MainManagerItemAttribute>();
        // id -> priority
        var priorityMap = new Dictionary<int, int>();
        foreach (var type in types)
        {
            if (typeof(IKimeraUI).IsAssignableFrom(type) is false)
            {
                Debug.LogWarning($"{type.FullName} 使用了 MainManagerItem 特性，但没有实现 IKimeraUI 接口");
                continue;
            }
            var obj = Activator.CreateInstance(type) as IKimeraUI;
            obj?.OnCreate();
            var attr = type.GetCustomAttribute<MainManagerItemAttribute>();
            var num = attr.ItemPath.LastIndexOf('/');
            var name = num is -1 ? attr.ItemPath : attr.ItemPath[(num + 1)..];
            var id = name.GetHashCode();
            var path = num is -1 ? "" : attr.ItemPath[..num];
            var priority = attr.Priority;
            priorityMap[id] = priority;
            var data = new KimeraMenuItemData(name, obj);
            var item = new TreeViewItemData<KimeraMenuItemData>(id, data);
            item.InsertToTreeAtPath(tree, path, child =>
            {
                var childPriority = priorityMap.TryGetValue(child.id, out var priority) ? priority : int.MaxValue;
                return priorityMap[id] < childPriority;
            });
        }
    }
}

#endif
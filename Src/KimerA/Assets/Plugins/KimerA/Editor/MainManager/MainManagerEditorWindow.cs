#if UNITY_EDITOR

using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using KimerA.Editor.UI;
using KimerA.Editor.Utils;

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

    protected override TreeViewItemDataBuilder<KimeraMenuItemData> OnMenuInit()
    {
        var tree = new TreeViewItemDataBuilder<KimeraMenuItemData>();
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

    private static void CollectMainManagerItems(TreeViewItemDataBuilder<KimeraMenuItemData> tree)
    {
        var types = TypeUtil.QueryTypesWithAttribute<MainManagerItemAttribute>();
        var root = new TreeViewItemDataBuilder<KimeraMenuItemData>();
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
            var itemPath = attr.ItemPath;
            var priority = attr.Priority;
            var id = itemPath.GetHashCode();
            priorityMap[id] = priority;
            var data = new KimeraMenuItemData(itemPath, obj);
            var helper = new ItemInsertHelper(id, data, priority);
            AddItemAtPath(root, helper, priorityMap);
        }

        tree.AddChildren(root.Children.ToList());
    }

    private readonly struct ItemInsertHelper
    {
        public readonly int Id;
        public readonly KimeraMenuItemData Data;
        public readonly int Priority;

        public ItemInsertHelper(int id, KimeraMenuItemData data, int priority)
        {
            Id = id;
            Data = data;
            Priority = priority;
        }
    }

    private static void AddItemAtPath(TreeViewItemDataBuilder<KimeraMenuItemData> node, ItemInsertHelper helper, Dictionary<int, int> priorityMap)
    {
        var parent = node;
        var walker = new PathWalker(helper.Data.Name);
        while (walker.MoveNext())
        {
            var current = walker.Current;
            foreach (var child in node.Children)
            {
                if (child.Data.Name == current)
                {
                    parent = node;
                    node = child;
                    goto FindItem;
                }
            }
            EnsurePathAdded(ref node, new KimeraMenuItemData(current.ToString(), null));

        FindItem:
            ;
        }

        var newNode = node.SetData(helper.Data);
        parent.ReplaceChild(newNode);

        void EnsurePathAdded(ref TreeViewItemDataBuilder<KimeraMenuItemData> parent, KimeraMenuItemData child)
        {
            var newChild = new TreeViewItemDataBuilder<KimeraMenuItemData>(child.Name.GetHashCode(), child);
            parent.InsertChildBy(newChild, child =>
            {
                var childPriority = priorityMap.TryGetValue(child.Id, out var priority) ? priority : int.MaxValue;
                return priorityMap[helper.Id] < childPriority;
            });
            parent = newChild;
        }
    }
}

#endif
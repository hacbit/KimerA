#if UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace KimerA.Editor.Utils;

/// <summary>
/// 工具类，用于处理树形结构相关的操作
/// </summary>
public static class TreeUtil
{
    public static void AddNodeAtPath<T>(this T tree, string path, T node, Func<string, T> paddingNodeCreator) where T : ITreeLike
    {
        var parent = tree;
        var current = tree;
        var walker = new PathWalker(path);
        while (walker.MoveNext())
        {
            var currentPath = walker.Current;
            foreach (var child in current.Children)
            {
                if (child.TreeNodePath == currentPath)
                {
                    parent = current;
                    current = (T)child;
                    goto FoundItem;
                }
            }

            // 找不到就创建一个新的节点
            EnsurePathAdded(ref parent, ref current, paddingNodeCreator(currentPath.ToString()));

        FoundItem:
            ;
        }

        parent.ReplaceChildBy(node, n => n.TreeNodePath == current.TreeNodePath);
    }

    private static void EnsurePathAdded<T>(ref T parent, ref T current, T defaultNode) where T : ITreeLike
    {
        current.AddChild(defaultNode);
        parent = current;
        current = defaultNode;
    }
}

public interface ITreeLike
{
    string TreeNodePath { get; }

    IEnumerable<ITreeLike> Children { get; }

    void AddChild(ITreeLike child);

    void AddChildren(IEnumerable<ITreeLike> children)
    {
        foreach (var child in children)
        {
            AddChild(child);
        }
    }

    void RemoveChildBy(Func<ITreeLike, bool> predicate);

    void ReplaceChildBy(ITreeLike newChild, Func<ITreeLike, bool> predicate);
}

#endif
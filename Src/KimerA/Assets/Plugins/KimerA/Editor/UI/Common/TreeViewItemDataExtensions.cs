#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace KimerA.Editor.UI;

public static class TreeViewItemDataExtensions
{
    public static IList<TreeViewItemData<T>> GetChildren<T>(this TreeViewItemData<T> item)
    {
        return item.children as List<TreeViewItemData<T>>;
    }

    public static void AddChild<T>(this TreeViewItemData<T> item, TreeViewItemData<T> child)
    {
        if (item.children is IList<TreeViewItemData<T>> m_Children)
        {
            m_Children.Add(child);
        }
    }

    public static void AddChldren<T>(this TreeViewItemData<T> item, IList<TreeViewItemData<T>> children)
    {
        if (item.children is IList<TreeViewItemData<T>> m_Children)
        {
            foreach (var child in children)
            {
                m_Children.Add(child);
            }
        }
    }

    public static void InsertChild<T>(this TreeViewItemData<T> item, TreeViewItemData<T> child, int index)
    {
        if (item.children is IList<TreeViewItemData<T>> m_Children)
        {
            if (index < 0 || index >= m_Children.Count)
            {
                m_Children.Add(child);
            }
            else
            {
                m_Children.Insert(index, child);
            }
        }
    }

    public static void InsertChildBy<T>(this TreeViewItemData<T> item, TreeViewItemData<T> newChild, Func<TreeViewItemData<T>, bool> comparer)
    {
        if (item.children is IList<TreeViewItemData<T>> m_Children)
        {
            for (int i = 0; i < m_Children.Count; i++)
            {
                if (comparer(m_Children[i]))
                {
                    m_Children.Insert(i, newChild);
                    return;
                }
            }

            m_Children.Add(newChild);
        }
    }

    public static void RemoveChild<T>(this TreeViewItemData<T> item, int childId)
    {
        if (item.children is IList<TreeViewItemData<T>> m_Children)
        {
            for (int i = 0; i < m_Children.Count; i++)
            {
                if (childId == m_Children[i].id)
                {
                    m_Children.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public static int GetChildIndex<T>(this TreeViewItemData<T> item, int itemId)
    {
        int num = 0;
        foreach (var child in item.children)
        {
            if (child.id == itemId)
            {
                return num;
            }
            num++;
        }
        return -1;
    }

    public static bool TryGetChild<T>(this TreeViewItemData<T> item, int itemId, out TreeViewItemData<T> child)
    {
        foreach (var _child in item.children)
        {
            if (_child.id == itemId)
            {
                child = _child;
                return true;
            }
        }
        child = new TreeViewItemData<T>();
        return false;
    }

    public static void ReplaceChild<T>(this TreeViewItemData<T> item, TreeViewItemData<T> newChild)
    {
        if (item.children is IList<TreeViewItemData<T>> m_Children)
        {
            int num = 0;
            foreach (TreeViewItemData<T> child in m_Children)
            {
                if (child.id == newChild.id)
                {
                    m_Children.RemoveAt(num);
                    m_Children.Insert(num, newChild);
                    break;
                }

                num++;
            }
        }
    }

    public static TreeViewItemData<T> ReplaceId<T>(this TreeViewItemData<T> item, int newId)
    {
        return new TreeViewItemData<T>(newId, item.data, item.children as List<TreeViewItemData<T>>);
    }

    public static TreeViewItemData<T> ReplaceData<T>(this TreeViewItemData<T> item, T newData)
    {
        return new TreeViewItemData<T>(item.id, newData, item.children as List<TreeViewItemData<T>>);
    }

    public static void AddToTreeAtPath<T>(this TreeViewItemData<T> self, TreeViewItemData<T> root, string path)
    {
        var current = root;
        var parts = path?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts is not null)
        {
            foreach (var part in parts)
            {
                var id = part.GetHashCode();
                if (current.TryGetChild(id, out var child))
                {
                    current = child;
                }
                else
                {
                    // add new node
                    var newChild = new TreeViewItemData<T>(id, default);
                    current.AddChild(child);
                    current = newChild;
                }
            }
        }

        current.AddChild(self);
    }

    public static void InsertToTreeAtPath<T>(this TreeViewItemData<T> self, TreeViewItemData<T> root, string path, Func<TreeViewItemData<T>, bool> comparer)
    {
        var current = root;
        var parts = path?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts is not null)
        {
            foreach (var part in parts)
            {
                var id = part.GetHashCode();
                if (current.TryGetChild(id, out var child))
                {
                    current = child;
                }
                else
                {
                    // add new node
                    var newChild = new TreeViewItemData<T>(id, default);
                    current.AddChild(child);
                    current = newChild;
                }
            }
        }

        current.InsertChildBy(self, comparer);
    }
}

#endif
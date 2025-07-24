#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace KimerA.Editor.UI;

public readonly struct TreeViewItemDataBuilder<T> : IEquatable<TreeViewItemDataBuilder<T>>
{
    public readonly int Id { get; init; }

    public readonly T Data { get; init; }

    private readonly IList<TreeViewItemDataBuilder<T>> m_Children { get; init; }

    public IEnumerable<TreeViewItemDataBuilder<T>> Children => m_Children;

    public bool HasChildren => m_Children is not null && m_Children.Count > 0;

    public TreeViewItemDataBuilder()
    {
        m_Children ??= new List<TreeViewItemDataBuilder<T>>();
    }

    public TreeViewItemDataBuilder(int id, T data, List<TreeViewItemDataBuilder<T>> children = null)
    {
        Id = id;
        Data = data;
        m_Children = children ?? new List<TreeViewItemDataBuilder<T>>();
    }

    public void AddChild(int id, T data, List<TreeViewItemDataBuilder<T>> children = null)
    {
        m_Children.Add(new TreeViewItemDataBuilder<T>(id, data, children));
    }

    public void AddChild(TreeViewItemDataBuilder<T> child)
    {
        m_Children.Add(child);
    }

    public void AddChildren(IList<TreeViewItemDataBuilder<T>> children)
    {
        foreach (var child in children)
        {
            AddChild(child);
        }
    }

    public void InsertChild(TreeViewItemDataBuilder<T> child, int index)
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

    public void InsertChildBy(TreeViewItemDataBuilder<T> child, Func<TreeViewItemDataBuilder<T>, bool> func)
    {
        for (int i = 0; i < m_Children.Count; i++)
        {
            if (func(m_Children[i]))
            {
                m_Children.Insert(i, child);
                return;
            }
        }
        m_Children.Add(child);
    }

    public void RemoveChild(int childId)
    {
        if (m_Children is null)
        {
            return;
        }
        for (int i = 0; i < m_Children.Count; i++)
        {
            if (childId == m_Children[i].Id)
            {
                m_Children.RemoveAt(i);
                break;
            }
        }
    }

    public int GetChildIndex(int itemId)
    {
        int num = 0;
        foreach (var child in m_Children)
        {
            if (child.Id == itemId)
            {
                return num;
            }
            num++;
        }
        return -1;
    }

    public void ReplaceChild(TreeViewItemDataBuilder<T> newChild)
    {
        if (HasChildren is false)
        {
            return;
        }
        int num = 0;
        foreach (var child in m_Children)
        {
            if (child.Id == newChild.Id)
            {
                m_Children.RemoveAt(num);
                m_Children.Insert(num, newChild);
                break;
            }
            num++;
        }
    }

    public TreeViewItemDataBuilder<T> SetId(int id)
    {
        return new TreeViewItemDataBuilder<T>(id, Data) { m_Children = m_Children };
    }

    public TreeViewItemDataBuilder<T> SetData(T data)
    {
        return new TreeViewItemDataBuilder<T>(Id, data) { m_Children = m_Children };
    }

    public TreeViewItemData<T> AsTreeViewItemData()
    {
        return new TreeViewItemData<T>(Id, Data, m_Children?.Select(child => child.AsTreeViewItemData()).ToList());
    }

    public bool Equals(TreeViewItemDataBuilder<T> other)
    {
        return Id == other.Id && EqualityComparer<T>.Default.Equals(Data, other.Data) && m_Children == other.m_Children;
    }

    public override bool Equals(object obj)
    {
        return obj is TreeViewItemDataBuilder<T> builder && Equals(builder);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Data, m_Children);
    }

    public static bool operator ==(TreeViewItemDataBuilder<T> left, TreeViewItemDataBuilder<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator != (TreeViewItemDataBuilder<T> left, TreeViewItemDataBuilder<T> right)
    {
        return !(left == right);
    }

    public static implicit operator TreeViewItemDataBuilder<T>(TreeViewItemData<T> item)
    {
        return new TreeViewItemDataBuilder<T>(
            item.id,
            item.data,
            item.hasChildren
                ? item.children.Select<TreeViewItemData<T>, TreeViewItemDataBuilder<T>>(child => child).ToList()
                : null
        );
    }
}


#endif
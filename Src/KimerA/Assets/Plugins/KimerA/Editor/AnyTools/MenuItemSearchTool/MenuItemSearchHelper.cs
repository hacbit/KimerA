#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace KimerA.Editor.AnyTools;

public static class MenuItemSearchHelper
{
    private static Dictionary<string, MenuItem> m_MenuItems;

    public static Dictionary<string, MenuItem> MenuItems
    {
        get
        {
            if (m_MenuItems is null)
            {
                var methods = TypeCache.GetMethodsWithAttribute<MenuItem>();
                var items = methods.SelectMany(static info => info.GetCustomAttributes(typeof(MenuItem), false).Cast<MenuItem>());
                m_MenuItems = new();
                foreach (var item in items)
                {
                    if (m_MenuItems.ContainsKey(item.menuItem) is false && item.validate is false)
                    {
                        m_MenuItems[item.menuItem] = item;
                    }
                }
            }
            return m_MenuItems;
        }
    }

    public static IEnumerable<MenuItem> GetAllMenuItem()
    {
        return MenuItems.Values;
    }

    public static IEnumerable<MenuItem> SearchMenuItems(string searchTerm)
    {
        foreach (var (itemPath, item) in MenuItems)
        {
            if (IsItemNameMatches(itemPath, searchTerm))
            {
                yield return item;
            }
        }
    }

    private static bool IsItemNameMatches(string itemName, string term)
    {
        int pos = 0;
        foreach (var c in term)
        {
            pos = itemName.IndexOf(c, pos);
            if (pos == -1)
            {
                return false;
            }
        }
        return true;
    }

    public static string HighlightItemPathByTerm(string itemName, string term)
    {
        var sb = new StringBuilder();
        var normalFormat = $"<color=#a0a0a0>{{0}}</color>";
        var highlightFormat = $"<b>{{0}}</b>";
        int lastPos = 0;
        int pos = 0;
        foreach (var c in term)
        {
            pos = itemName.IndexOf(c, pos);
            if (pos == -1) continue;
            if (pos > lastPos)
            {
                sb.AppendFormat(normalFormat, itemName[lastPos..pos]);
                sb.AppendFormat(highlightFormat, c);
                lastPos = pos + 1;
            }
        }
        if (lastPos < itemName.Length)
        {
            sb.AppendFormat(normalFormat, itemName[lastPos..]);
        }
        return sb.ToString();
    }
}

#endif
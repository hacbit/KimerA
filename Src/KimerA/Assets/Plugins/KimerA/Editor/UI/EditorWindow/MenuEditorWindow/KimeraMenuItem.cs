#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace KimerA.Editor.UI
{
    public readonly struct KimeraMenuItemTree
    {
        public KimeraMenuItemTree(List<TreeViewItemData<KimeraMenuItemData>> items)
        {
            Items = items;
        }

        public KimeraMenuItemTree()
        {
            Items ??= new List<TreeViewItemData<KimeraMenuItemData>>();
        }

        public readonly List<TreeViewItemData<KimeraMenuItemData>> Items;

        public void Add(TreeViewItemData<KimeraMenuItemData> itemData)
        {
            Items.Add(itemData);
        }

        public void Add(int id, string name, IKimeraUI value)
        {
            Items.Add(new TreeViewItemData<KimeraMenuItemData>(id, new(name, value)));
        }
    }

    public readonly struct KimeraMenuItemData
    {
        public readonly string Name;
        public readonly IKimeraUI Value;

        public KimeraMenuItemData(string name, IKimeraUI value)
        {
            Name = name;
            Value = value;
        }
    }
}

#endif
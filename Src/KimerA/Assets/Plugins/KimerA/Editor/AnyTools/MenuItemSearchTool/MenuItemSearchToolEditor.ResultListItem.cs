#if UNITY_EDITOR

using UnityEditor.UIElements;
using UnityEngine.UIElements;
using KimerA.Editor.Utils;

namespace KimerA.Editor.AnyTools;

public sealed partial class MenuItemSearchToolEditor
{
    private const string RESULT_LIST_ITEM_VISUAL_TREE_PATH = "AnyTools/MenuItemSearchTool/UI/SearchResultListItemUI.uxml";

    private VisualTreeAsset m_ResultListItemVisualTreeAsset = null;

    public VisualElement GetDefaultListItem()
    {
        if (m_ResultListItemVisualTreeAsset is null)
        {
            m_ResultListItemVisualTreeAsset = ResUtil.ResolveEditorAsset<VisualTreeAsset>(RESULT_LIST_ITEM_VISUAL_TREE_PATH);
        }
        return m_ResultListItemVisualTreeAsset.CloneTree();
    }

    public struct ResultListItem
    {
        public VisualElement Root { get; }

        public Label ItemName { get; }

        public Label ItemPath { get; }

        public ToolbarButton OpenButton { get; }

        public ToolbarButton StarButton { get; }

        public ResultListItem(VisualElement root)
        {
            Root = root;
            ItemName = root.Q<Label>("ItemName");
            ItemPath = root.Q<Label>("ItemPath");
            OpenButton = root.Q<ToolbarButton>("OpenButton");
            StarButton = root.Q<ToolbarButton>("StarButton");
        }
    }
}

#endif
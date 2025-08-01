#if UNITY_EDITOR

using System.Linq;
using KimerA.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace KimerA.Editor.UI
{
    public readonly struct KimeraMenuItemData
    {
        public string Name { get; }
        public IKimeraUI Value { get; }

        public KimeraMenuItemData(string name, IKimeraUI value)
        {
            Name = name;
            Value = value;
        }
    }

    public abstract class KimeraMenuEditorWindow : EditorWindow
    {
        private VisualTreeAsset m_VisualTreeAsset = default;

        private const string VISUAL_TREE_ASSET_PATH = "UI/EditorWindow/MenuEditorWindow/UI/KimeraMenuEditorWindow.uxml";

        protected TreeView MenuArea;

        protected ScrollView ContentArea;

        protected Toolbar ToolbarArea;

        public TreeViewItemData<KimeraMenuItemData> MenuTree { get; private set; }

        private void OnEnable()
        {
            m_VisualTreeAsset = ResUtil.ResolveEditorAsset<VisualTreeAsset>(VISUAL_TREE_ASSET_PATH);

            VisualElement root = rootVisualElement;

            m_VisualTreeAsset.CloneTree(root);
            root.StretchToParentSize();

            MenuArea = root.Q<TreeView>("MenuArea");
            ContentArea = root.Q<ScrollView>("ContentArea");
            ToolbarArea = root.Q<Toolbar>("ToolbarArea");

            InitMenu_Internal();
        }

        protected abstract TreeViewItemData<KimeraMenuItemData> OnMenuInit();

        private void InitMenu_Internal()
        {
            MenuTree = OnMenuInit();
            MenuArea.SetRootItems(MenuTree.GetChildren());
            MenuArea.makeItem = static () => new Label();
            MenuArea.bindItem = (e, i) =>
            {
                var item = MenuArea.GetItemDataForIndex<KimeraMenuItemData>(i);
                (e as Label).text = item.Name;
                e.parent.parent.style.alignItems = new StyleEnum<Align>(Align.Center);
            };
            MenuArea.selectionChanged += (selections) =>
            {
                var data = selections.FirstOrDefault();
                if (data is KimeraMenuItemData selection)
                {
                    ContentArea.Clear();
                    selection.Value?.OnEnable();
                    if (selection.Value?.RootElement is not null)
                    {
                        ContentArea.Add(selection.Value?.RootElement);
                    }
                }
            };
            MenuArea.selectionType = SelectionType.Single;
            MenuArea.Rebuild();
        }

        public void RefreshMenu()
        {
            MenuArea.SetRootItems(MenuTree.GetChildren());
            MenuArea.Rebuild();
        }
    }
}

#endif
#if UNITY_EDITOR

using System.Linq;
using KimerA.Editor.Utils;
using UnityEditor;
using UnityEngine.UIElements;

namespace KimerA.Editor.UI
{
    public abstract class KimeraMenuEditorWindow : EditorWindow
    {
        private VisualTreeAsset m_VisualTreeAsset = default;

        private const string VISUAL_TREE_ASSET_PATH = "UI/EditorWindow/MenuEditorWindow/UI/KimeraMenuEditorWindow.uxml";

        protected TreeView MenuArea;

        protected ScrollView ContentArea;

        public KimeraMenuItemTree MenuTree { get; private set; }

        private void OnEnable()
        {
            m_VisualTreeAsset = PathUtil.ResolveEditorAsset<VisualTreeAsset>(VISUAL_TREE_ASSET_PATH);

            VisualElement root = rootVisualElement;

            var splitView = m_VisualTreeAsset.CloneTree();
            splitView.StretchToParentSize();
            root.Add(splitView);

            MenuArea = splitView.Q<TreeView>("MenuArea");
            ContentArea = splitView.Q<ScrollView>("ContentArea");

            InitMenu_Internal();
        }

        protected abstract KimeraMenuItemTree OnMenuInit();

        private void InitMenu_Internal()
        {
            MenuTree = OnMenuInit();
            MenuArea.SetRootItems(MenuTree.Items);
            MenuArea.makeItem = static () => new Label();
            MenuArea.bindItem = (e, i) =>
            {
                var item = MenuArea.GetItemDataForIndex<KimeraMenuItemData>(i);
                (e as Label).text = item.Name;
                e.parent.parent.style.alignItems = new StyleEnum<Align>(Align.Center);
            };
            MenuArea.selectionChanged += (selections) =>
            {
                var data = GetSelectionItemData();
                ContentArea.Clear();
                data.Value?.OnEnable();
                if (data.Value?.RootElement is not null)
                {
                    ContentArea.Add(data.Value.RootElement);
                }
            };
            MenuArea.selectionType = SelectionType.Single;
            MenuArea.Rebuild();

            if (MenuTree.Items.Count > 0)
            {
                var id = MenuTree.Items[0].id;
                MenuArea.SetSelectionById(id);
            }
        }

        public KimeraMenuItemData GetSelectionItemData()
        {
            var item = MenuArea.GetSelectedItems<KimeraMenuItemData>().FirstOrDefault();
            return item.data;
        }

        public void RefreshMenu()
        {
            MenuArea.SetRootItems(MenuTree.Items);
            MenuArea.Rebuild();
        }

        protected virtual void OnGUI()
        {
            var item = GetSelectionItemData();
            item.Value?.OnUpdate();
        }
    }
}

#endif
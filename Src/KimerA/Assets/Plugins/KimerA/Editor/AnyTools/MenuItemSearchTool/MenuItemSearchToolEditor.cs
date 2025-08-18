#if UNITY_EDITOR

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KimerA.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KimerA.Editor.AnyTools;

public sealed partial class MenuItemSearchToolEditor : EditorWindow
{
    [MenuItem("KimerA/工具快速查找")]
    public static void Open()
    {
        var window = GetWindow<MenuItemSearchToolEditor>();
        window.titleContent = new GUIContent("工具快速查找");
        window.Show();
    }

    private const string VISUAL_TREE_ASSET_PATH = "AnyTools/MenuItemSearchTool/UI/MenuItemSearchToolUI.uxml";

    public TextField SearchField { get; private set; }

    public ListView SearchResult { get; private set; }

    private readonly List<MenuItem> m_FilteredResult = new();

    private void OnEnable()
    {
        var root = rootVisualElement;
        var visualTree = ResUtil.ResolveEditorAsset<VisualTreeAsset>(VISUAL_TREE_ASSET_PATH);
        visualTree.CloneTree(root);
        root.StretchToParentSize();

        SearchField = root.Q<TextField>("SearchField");
        BindSearchField(SearchField);
        SearchResult = root.Q<ListView>("SearchResult");
        BindSearchResult(SearchResult);
    }

    private void BindSearchField(TextField searchField)
    {
        searchField.RegisterValueChangedCallback(OnSearchTermChanged);
    }

    private void BindSearchResult(ListView searchResult)
    {
        searchResult.itemsSource = m_FilteredResult;
        searchResult.makeItem += GetDefaultListItem;
        searchResult.bindItem += (elem, i) =>
        {
            var helper = new ResultListItem(elem);
            var data = m_FilteredResult[i];

            var path = data.menuItem;
            var name = path.Split('/')[^1];
            helper.ItemName.text = name;
            helper.ItemPath.text = MenuItemSearchHelper.HighlightItemPathByTerm(path, SearchField.value);
            helper.OpenButton.clicked += () =>
            {
                if (EditorApplication.ExecuteMenuItem(path))
                {
                    Debug.Log($"[{nameof(MenuItemSearchToolEditor)}] 正在打开 {path}");
                }
                else
                {
                    Debug.LogWarning($"[{nameof(MenuItemSearchToolEditor)}] 打开 {path} 失败");
                }
            };
            helper.StarButton.clicked += () => { };
        };
    }

    private void OnSearchTermChanged(ChangeEvent<string> @event)
    {
        if (@event.previousValue.Trim() != @event.newValue.Trim())
        {
            ApplySearch(@event.newValue);
        }
    }

    public void ApplySearch(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            m_FilteredResult.Clear();
            SearchResult.Rebuild();
            return;
        }

        var results = MenuItemSearchHelper.SearchMenuItems(searchTerm);
        m_FilteredResult.Clear();
        m_FilteredResult.AddRange(results);
        SearchResult.Rebuild();
    }
}

#endif
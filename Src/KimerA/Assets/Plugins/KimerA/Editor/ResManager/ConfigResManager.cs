#if UNITY_EDITOR

using UnityEngine.UIElements;
using KimerA.Editor.UI;
using KimerA.Editor.Utils;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KimerA.Editor;

[MainManagerItem("Config Res", Priority = 10)]
public sealed class ConfigResManager : IKimeraUI
{
    public VisualElement RootElement { get; private set; } = new VisualElement();

    private EditorStorageSO[] _configStorages;

    public void OnCreate()
    {
        var visualTreeAsset = ResUtil.ResolveEditorAsset<VisualTreeAsset>("ResManager/UI/ConfigResManager.uxml");
        visualTreeAsset.CloneTree(RootElement);
    }

    public void OnEnable()
    {
        var root = RootElement;
        var configDropdown = root.Q<DropdownField>("ConfigDropdown");
        var configDisplayTree = root.Q<TreeView>("ConfigDisplayTree");

        Init(configDropdown);

        configDropdown.RegisterValueChangedCallback(evt =>
        {
            var selectedStorage = _configStorages.FirstOrDefault(s => s.name == evt.newValue);
            if (selectedStorage is not null)
            {
                configDisplayTree.SetRootItems(ResolveAllDebugableValues(selectedStorage));
            }
            else
            {
                configDisplayTree.SetRootItems(new List<TreeViewItemData<string>>());
            }
            configDisplayTree.Rebuild();
            configDisplayTree.ExpandAll();
        });

        if (_configStorages.Length > 0)
        {
            var first = _configStorages[0];
            configDisplayTree.SetRootItems(ResolveAllDebugableValues(first));
            configDisplayTree.Rebuild();
            configDisplayTree.ExpandAll();
        }
    }

    private void Init(DropdownField dropdown)
    {
        _configStorages = ResUtil.ResolveConfigResAllAssets<EditorStorageSO>().ToArray();
        dropdown.choices = _configStorages.Select(s => s.name).ToList();
        dropdown.value = _configStorages.FirstOrDefault()?.name ?? string.Empty;
    }

    private IList<TreeViewItemData<string>> ResolveAllDebugableValues(EditorStorageSO storage)
    {
        var list = new List<TreeViewItemData<string>>();
        foreach (var (type, typeData) in storage.m_StorageDatas)
        {
            foreach (var (key, value) in typeData)
            {
                var item = new TreeViewItemData<string>(key.GetHashCode(), $"{key} = {JsonConvert.SerializeObject(value)}");
                list.Add(item);
            }
        }
        return list;
    }
}

#endif
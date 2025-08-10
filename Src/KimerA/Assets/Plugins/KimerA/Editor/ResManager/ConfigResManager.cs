#if UNITY_EDITOR

using UnityEngine.UIElements;
using KimerA.Editor.UI;
using KimerA.Editor.Utils;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KimerA.Editor;

[MainManagerItem("Config Res", Priority = 0)]
public sealed class ConfigResManager : IKimeraUI
{
    public VisualElement RootElement { get; private set; } = new VisualElement();

    internal List<string> _configStorages { get; private set; }

    public void OnCreate()
    {
        var visualTreeAsset = ResUtil.ResolveEditorAsset<VisualTreeAsset>("ResManager/UI/ConfigResManager.uxml");
        visualTreeAsset.CloneTree(RootElement);
    }

    public void OnEnable()
    {
        _configStorages = new[] { "" }.Concat(EditorStorageUtil.GetAllStorageKeys()).ToList();

        var root = RootElement;
        var configDropdown = root.Q<DropdownField>("ConfigDropdown");
        var configDisplayTree = root.Q<TreeView>("ConfigDisplayTree");

        Init(configDropdown);

        configDropdown.RegisterValueChangedCallback(evt =>
        {
            var selectedKey = _configStorages.FirstOrDefault(s => s == evt.newValue);
            if (string.IsNullOrEmpty(selectedKey) is false)
            {
                var config = EditorStorageUtil.GetStorage(selectedKey);
                configDisplayTree.SetRootItems(ResolveAllDebugableValues(config));
            }
            else
            {
                configDisplayTree.SetRootItems(new List<TreeViewItemData<string>>());
            }
            configDisplayTree.Rebuild();
            configDisplayTree.ExpandAll();
        });
    }

    private void Init(DropdownField dropdown)
    {
        dropdown.choices = _configStorages;
        dropdown.value = _configStorages.FirstOrDefault() ?? string.Empty;
    }

    private IList<TreeViewItemData<string>> ResolveAllDebugableValues(Dictionary<string, object> storage)
    {
        var list = new List<TreeViewItemData<string>>();
        foreach (var (name, value) in storage)
        {
            var item = new TreeViewItemData<string>(name.GetHashCode(), $"{name} = {JsonConvert.SerializeObject(value)}");
            list.Add(item);
        }
        return list;
    }
}

#endif
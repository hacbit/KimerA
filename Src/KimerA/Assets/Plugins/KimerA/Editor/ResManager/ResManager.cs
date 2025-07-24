#if UNITY_EDITOR

using KimerA.Editor.UI;
using KimerA.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace KimerA.Editor;

[MainManagerItem("Resource", Priority = 1)]
public sealed class ResManager : IKimeraUI
{
    public VisualElement RootElement { get; private set; } = new VisualElement();

    public void OnCreate()
    {
        var visualTreeAsset = PathUtil.ResolveEditorAsset<VisualTreeAsset>("ResManager/UI/ResManager.uxml");
        visualTreeAsset.CloneTree(RootElement);
    }
}

#endif
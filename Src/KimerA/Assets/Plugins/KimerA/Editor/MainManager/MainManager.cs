#if UNITY_EDITOR

using KimerA.Editor.UI;
using UnityEngine.UIElements;

namespace KimerA.Editor;

[MainManagerItem("Main", Priority = 0)]
public sealed class MainManager : IKimeraUI
{
    public string Name => "Main";

    public VisualElement RootElement { get; private set; } = new VisualElement();

    public void OnCreate()
    {
        var root = RootElement;
        root.Add(new Label("This is Main Manager Content"));
    }
}

#endif
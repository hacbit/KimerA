using UnityEngine.UIElements;

namespace KimerA.Editor.UI;

public interface IKimeraUI
{
    VisualElement RootElement { get; }

    void OnCreate();

    void OnEnable() { }
}
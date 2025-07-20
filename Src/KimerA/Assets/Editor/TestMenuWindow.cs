#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using KimerA.Editor.UI;
using UnityEngine.UIElements;

namespace Editor.Test;

public sealed class TestMenuWindow : KimeraMenuEditorWindow
{
    [MenuItem("Test/Test Menu Window")]
    public static void Open()
    {
        var window = GetWindow<TestMenuWindow>();
        window.titleContent = new GUIContent("Test Menu Window");
        window.Show();
    }

    protected override KimeraMenuItemTree OnMenuInit()
    {
        var list = new KimeraMenuItemTree();
        for (int i = 1; i < 10; i++)
        {
            list.Add(i, $"item {i}", new TestData() { Name = $"TestData At {i}"});
        }
        return list;
    }

    public class TestData : IKimeraUI
    {
        public string Name;

        public VisualElement RootElement { get; private set; }

        public void OnEnable()
        {
            RootElement = new VisualElement();
            var button = new Button(() => Debug.Log($"click {Name}!"))
            {
                name = "click me",
            };
            button.style.height = 20f;
            var label = new Label($"This {Name}");
            label.style.alignSelf = new StyleEnum<Align>(Align.Center);
            button.Add(label);
            RootElement.Add(button);
        }
    }
}


#endif
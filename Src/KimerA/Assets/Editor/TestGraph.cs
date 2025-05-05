#if UNITY_EDITOR

using KimerA.Editor.Graph;
using UnityEngine;
using UnityEditor;

public class TestGraph : BaseGraphEditor<TestGraphNode>
{
    [MenuItem("KimerA/Test Graph")]
    public static void OpenWindow()
    {
        var window = GetWindow<TestGraph>();
        window.titleContent = new GUIContent("Test Graph");
        window.Show();
    }

    public override GraphMenuTree BuildContextMenu()
    {
        var addMenu = new GraphMenuTree("Add Node")
        {
            { "A Node", CreateANode },
            { "B Node", CreateBNode },
        };
        return addMenu;
    }

    private BaseGraphNode CreateANode()
    {
        return new("A Node", Event.current.mousePosition, 200, 100);
    }

    private BaseGraphNode CreateBNode()
    {
        return new("B Node", Event.current.mousePosition, (150, 75));
    }
}


public class TestGraphNode : BaseGraphNode
{

}

#endif
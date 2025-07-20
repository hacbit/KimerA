#if UNITY_EDITOR

using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace KimerA.Editor.UI;

public class GraphView_Helper : GraphView
{
    public new class UxmlFactory : UxmlFactory<GraphView_Helper, UxmlTraits> { }
}

#endif
#if UNITY_EDITOR

using UnityEngine.UIElements;

namespace KimerA.Editor.UI;

/// <summary>
/// Expose TwoPaneSplitView to UI Builder
/// </summary>
public class TwoPaneSplitView_Helper : TwoPaneSplitView
{
    public new class UxmlFactory : UxmlFactory<TwoPaneSplitView_Helper, UxmlTraits> { }
}

#endif
#if UNITY_EDITOR

namespace KimerA.Data.Res
{
    using System;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;

    public sealed class ResEditorWindow : OdinEditorWindow
    {
        [MenuItem("KimerA/Res Editor")]
        private static void ShowWindow()
        {
            GetWindow<ResEditorWindow>("Res Editor").Show();
        }

        public ResSetting ResSetting = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            
        }
    }
}

#endif
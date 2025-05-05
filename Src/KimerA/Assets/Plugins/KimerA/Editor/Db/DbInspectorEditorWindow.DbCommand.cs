#if UNITY_EDITOR && ODIN_INSPECTOR

using Sirenix.OdinInspector;

namespace KimerA.Db
{
    public sealed partial class DbInspectorEditorWindow
    {
        public sealed class DbCommand
        {
            [ShowInInspector]
            [BoxGroup("SQL Command")]
            private readonly string sql = string.Empty;

            [BoxGroup("SQL Command")]
            [Title("Result")]
            [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker]
            private SqlResult result = new();

            [Button]
            private void Execute()
            {
                result = DbControl.ExecuteReadonly(sql);
            }
        }
    }
}

#endif
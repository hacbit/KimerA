#if UNITY_EDITOR

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace KimerA.Test
{
    public sealed class TestValueDropdown : MonoBehaviour
    {
        [ShowInInspector, LabelText("测试下拉框")]
        [ValueDropdownEx(nameof(GetAllowIndexs), nameof(HandleItem))]
        private int Index;

        private ValueDropdownList<int> GetAllowIndexs()
        {
            var list = new ValueDropdownList<int>();
            for (int i = 0; i < 10; i++)
            {
                list.Add($"The {i + 1} item", i);
            }
            return list;
        }

        private void HandleItem(OdinMenuItem item)
        {
            item.Icon = EditorIcons.Flag.Active;
        }
    }
}

#endif
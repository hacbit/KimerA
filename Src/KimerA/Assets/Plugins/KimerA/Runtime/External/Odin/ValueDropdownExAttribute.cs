#if UNITY_EDITOR && ODIN_INSPECTOR

using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class ValueDropdownExAttribute : ValueDropdownAttribute
    {
        public string? ItemAction;

        public ValueDropdownExAttribute(string valueGetter, string? itemAction = null)
            : base(valueGetter)
        {
            ItemAction = itemAction;
        }
    }
}

#endif
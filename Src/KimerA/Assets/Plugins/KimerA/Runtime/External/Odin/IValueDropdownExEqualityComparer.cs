#if UNITY_EDITOR && ODIN_INSPECTOR

using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Drawers
{
    internal class IValueDropdownExEqualityComparer : IEqualityComparer<object>
    {
        private bool isTypeLookup;

        public IValueDropdownExEqualityComparer(bool isTypeLookup)
        {
            this.isTypeLookup = isTypeLookup;
        }

        public new bool Equals(object x, object y)
        {
            if (x is ValueDropdownItem itemX) x = itemX.Value;
            if (y is ValueDropdownItem itemY) y = itemY.Value;

            if (EqualityComparer<object>.Default.Equals(x, y))
                return true;

            if (x is null ^ y is null) return false;

            if (isTypeLookup)
            {
                var type = (x as Type) ?? x.GetType();
                var type2 = (y as Type) ?? y.GetType();
                if (type == type2) return true;
            }

            return false;
        }

        public int GetHashCode(object obj)
        {
            if (obj is null) return -1;

            if (obj is ValueDropdownItem item) obj = item.Value;

            if (obj is null) return -1;

            if (isTypeLookup)
            {
                var type = (obj as Type) ?? obj.GetType();
                return type.GetHashCode();
            }

            return obj.GetHashCode();
        }
    }
}

#endif
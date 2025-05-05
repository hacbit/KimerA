#if ODIN_INSPECTOR

namespace KimerA.Data.Excel.Config
{
    using System;
    using System.Linq;
    using KimerA.Utils;
    using Newtonsoft.Json;
    using Sirenix.OdinInspector;

    [Serializable]
    public struct ExcelStructField
    {
        public const string Error = "Structure field type does not support List";

        [LabelText("Field name")]
        public string Name;

        [ValueDropdown("@ExcelEditorWindow.ExcelTypes")]
        [ValidateInput("CheckValid", Error)]
        [LabelText("Field type")]
        public string Type;

        [ValidateInput("CheckValidBool", Error)]
        [LabelText("Is List")]
        public bool IsList;

        [JsonIgnore]
        public readonly bool IsStruct =>
            TypeUtil.DefaultTypes.Contains(Type) == false
            || TypeUtil.ResTypes.Contains(Type) == false;

        private readonly bool CheckValid(string type, ref string err)
        {
            if (IsStruct == false || IsList == false)
            {
                return true;
            }
            err = Error;
            return false;
        }

        private readonly bool CheckValidBool(bool type, ref string err)
        {
            if (IsStruct == false || IsList == false)
            {
                return true;
            }
            err = Error;
            return false;
        }
    }
}

#endif
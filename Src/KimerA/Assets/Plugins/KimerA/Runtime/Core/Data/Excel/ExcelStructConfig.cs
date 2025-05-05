#if ODIN_INSPECTOR

namespace KimerA.Data.Excel.Config
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;

    [Serializable]
    public struct ExcelStructConfig
    {
        [LabelText("Structure namespace")]
        public string Namespace;

        [LabelText("Structure name")]
        public string Name;

        [LabelText("Structure fields")]
        public List<ExcelStructField> Fields;

        public ExcelStructConfig(string name, string ns, List<ExcelStructField> fields)
        {
            Namespace = ns;
            Name = name;
            Fields = fields ?? new();
        }
    }
}

#endif
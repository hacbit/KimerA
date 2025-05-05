#if UNITY_EDITOR && ODIN_INSPECTOR

namespace KimerA.Data.Excel.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using KimerA.Data.Excel.Config;
    using KimerA.Utils;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    public sealed partial class ExcelEditorWindow : OdinEditorWindow
    {
        [MenuItem("KimerA/Excel Editor")]
        public static void ShowWindow()
        {
            GetWindow<ExcelEditorWindow>("Excel Editor").Show();
        }

        [Header("Setting")]
        [ShowInInspector]
        [VerticalGroup("Setting")]
        [Space(10)]
        public static readonly ExcelSetting ExcelSetting = new();

        public static IEnumerable<string> ExcelTypes
        {
            get
            {
                var res = ExcelSetting.StructConfigs.Select(cfg => cfg.Name).ToList();
                res.AddRange(TypeUtil.DefaultTypes);
                res.AddRange(TypeUtil.ResTypes);
                return res;
            }
        }

        [HorizontalGroup("Func")]
        [Button("Auto Import Structure", ButtonSizes.Medium)]
        [Tooltip("Auto import all structures which implement <IExcelStruct> interface to excel setting.")]
        private void ImportStructure()
        {
            var types = TypeUtil.GetDerivedStructs(typeof(IExcelStruct));
            ExcelSetting.StructConfigs = types.Select(ty =>
            {
                var fields = ty.GetSerializableFields();
                var excelStructConfig = new ExcelStructConfig
                {
                    Namespace = ty.Namespace,
                    Name = ty.Name,
                    Fields = new(),
                };
                foreach (var field in fields)
                {
                    var fieldName = field.FieldType.IsGenericType
                        && field.FieldType.GetGenericTypeDefinition() == typeof(List<>)
                        ? field.FieldType.GenericTypeArguments[0].Name
                        : field.FieldType.Name;
                    excelStructConfig.Fields.Add(new ExcelStructField
                    {
                        Name = field.Name,
                        Type = fieldName,
                        IsList = field.FieldType.IsGenericType
                    });
                }

                return excelStructConfig;
            }).ToList();
        }

        private string GetExcelFieldNamespace(string type)
        {
            if (ExcelTypes.Contains(type) == false)
            {
                return string.Empty;
            }
            var config = ExcelSetting.StructConfigs.FirstOrDefault(cfg => cfg.Name == type);
            return config.Namespace;
        }
    }
}

#endif
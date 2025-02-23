#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace KimerA.Db
{
    public sealed partial class DbInspectorEditorWindow
    {
        public sealed class TableViewer
        {
            [ShowInInspector]
            [BoxGroup("Table Viewer")]
            [ValueDropdown(nameof(GetTableNames))]
            [OnValueChanged(nameof(SetTableConfig))]
            [InlineButton(nameof(Query), " Query ", ShowIf = nameof(CanQuery))]
            [Tooltip("Select a table to view.")]
            public static string TableName = string.Empty;

            [ShowInInspector]
            [BoxGroup("Table Viewer")]
            [Title("Result")]
            [InlineProperty, HideLabel, HideReferenceObjectPicker]
            public static SqlResult Result = new();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<string> GetTableNames()
            {
                return DbControl.GetAllTables();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool CanQuery()
            {
                return TableName.IsNullOrWhitespace() == false;
            }

            private void Query()
            {
                Result = DbControl.ExecuteReadonly($"SELECT * FROM {TableName}");
            }

            private static TableManager.TableConfig tableConfig;

            private static void SetTableConfig()
            {
                tableConfig = TableManager.TableConfigs.Find(x => x.TableName == TableName);
                SetTableForm();
            }

            [Serializable]
            public struct TableForm
            {
                [ReadOnly]
                public string[] ColumnNames;
                [ReadOnly]
                public Type[] ColumnTypes;
                public object?[] ColumnValues;
            }

            [ShowInInspector]
            [BoxGroup("Table Viewer")]
            [Title("Table Form")]
            [InlineProperty, HideLabel, HideReferenceObjectPicker]
            public static TableForm tableForm;

            public static void DrawTableForm()
            {
                if (tableForm.ColumnNames == null) return;

                for (int i = 0; i < tableForm.ColumnNames.Length; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(tableForm.ColumnNames[i], GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(tableForm.ColumnTypes[i].Name, GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                    
                }                
            }

            public static void SetTableForm()
            {
                tableForm.ColumnNames = new string[tableConfig.Columns.Count];
                tableForm.ColumnTypes = new Type[tableConfig.Columns.Count];
                tableForm.ColumnValues = new object[tableConfig.Columns.Count];

                for (int i = 0; i < tableConfig.Columns.Count; i++)
                {
                    tableForm.ColumnNames[i] = tableConfig.Columns[i].Name;
                    try
                    {
                        var type = Type.GetType(tableConfig.Columns[i].CsType) ?? Type.GetType("System." + tableConfig.Columns[i].CsType);
                        tableForm.ColumnTypes[i] = type;
                    }
                    catch (Exception)
                    {
                        tableForm.ColumnTypes[i] = typeof(object);
                    }

                    try
                    {
                        tableForm.ColumnValues[i] = Activator.CreateInstance(tableForm.ColumnTypes[i]);
                    }
                    catch (Exception)
                    {
                        tableForm.ColumnValues[i] = null;
                    }
                }
            }

            public void AddRow()
            {
                
            }
        }
    }
}

#endif
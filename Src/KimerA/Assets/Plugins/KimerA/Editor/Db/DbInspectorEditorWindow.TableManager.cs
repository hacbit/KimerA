#if UNITY_EDITOR && ODIN_INSPECTOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using KimerA.Code;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;

namespace KimerA.Db
{
    public sealed partial class DbInspectorEditorWindow
    {
        public sealed class TableManager
        {
            public TableManager()
            {
                LoadTableConfig();
            }

            [Serializable]
            public struct TableConfig
            {
                public string TableName;
                [TableList]
                public List<Column> Columns;

                public override readonly bool Equals(object obj)
                {
                    return obj is TableConfig config && TableName == config.TableName;
                }

                public override readonly int GetHashCode()
                {
                    return TableName.GetHashCode();
                }
            }

            [Serializable]
            public struct Column
            {
                public string Name;
                [ValueDropdown(nameof(GetAllowedCsTypes))]
                [OnValueChanged(nameof(UpdateSqlType))]
                public string CsType;
                [ReadOnly]
                public SqlDataType SqlType;

                public readonly bool IsNull()
                {
                    return Name.IsNullOrWhitespace() || CsType.IsNullOrWhitespace();
                }

                private readonly IEnumerable<string> GetAllowedCsTypes => 
                    TypeConfiguration.AllowedDefaultCsTypes.Concat(TypeConfiguration.AllowedAdditionalCsTypes);

                private void UpdateSqlType()
                {
                    SqlType = CsType switch
                    {
                        nameof(SByte) or nameof(Byte)
                        or nameof(Int16) or nameof(UInt16)
                        or nameof(Int32) or nameof(UInt32)
                        or nameof(Int64) or nameof(UInt64)
                        or nameof(UIntPtr) or nameof(IntPtr)
                        or nameof(Decimal) or nameof(Boolean) or nameof(Char)
                            => SqlDataType.Interger,
                        nameof(String) or nameof(DateTime) or nameof(TimeSpan) or nameof(Guid)
                            => SqlDataType.Text,
                        nameof(Single) or nameof(Double)
                            => SqlDataType.Real,
                        _ => SqlDataType.Blob
                    };
                }
            }

            [ShowInInspector, ReadOnly]
            [BoxGroup("Table Config")]
            [ListDrawerSettings(ShowFoldout = true)]
            public static List<TableConfig> TableConfigs = new();

            [ShowInInspector]
            [BoxGroup("Table Config")]
            [BoxGroup("Table Config/New Table Config")]
            [InlineProperty, HideLabel]
            public TableConfig NewTableConfig = new()
            {
                TableName = "NewTable",
                Columns = new()
            };

            [BoxGroup("Table Config")]
            [BoxGroup("Table Config/New Table Config")]
            [Button("Add New Table Config", Style = ButtonStyle.Box)]
            private void AddNewColumn()
            {
                if (IsValidName(NewTableConfig.TableName) == false)
                {
                    EditorUtility.DisplayDialog("Error", "Table name is invalidate", "OK");
                    return;
                }
                var tableName = NewTableConfig.TableName;
                if (TableConfigs.Any(c => c.TableName == tableName))
                {
                    EditorUtility.DisplayDialog("Error", "This table is already exist", "OK");
                    return;
                }
                if (NewTableConfig.Columns.Count == 0)
                {
                    EditorUtility.DisplayDialog("Error", "Column is empty", "OK");
                    return;
                }
                foreach (var column in NewTableConfig.Columns)
                {
                    if (IsValidName(column.Name) == false)
                    {
                        EditorUtility.DisplayDialog("Error", "Column name is invalidate", "OK");
                        return;
                    }
                    if (column.CsType.IsNullOrWhitespace())
                    {
                        EditorUtility.DisplayDialog("Error", "Column type is invalidate", "OK");
                        return;
                    }
                    if (column.SqlType == SqlDataType.Null)
                    {
                        EditorUtility.DisplayDialog("Error", "Column type is invalidate", "OK");
                        return;
                    }
                }

                var copied = new TableConfig
                {
                    TableName = tableName,
                    Columns = NewTableConfig.Columns.Select(c => new Column
                    {
                        Name = c.Name,
                        CsType = c.CsType,
                        SqlType = c.SqlType
                    }).ToList()
                };

                TableConfigs.Add(copied);
            }

            /// <summary>
            /// Check if the table name is valid
            /// <para>Table name should not be null/empty or contain whitespace</para>
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsValidName(string name)
            {
                return string.IsNullOrEmpty(name) == false && name.Any(char.IsWhiteSpace) == false;
            }

            [BoxGroup("Table Config")]
            [Button("Remove Table Config", Expanded = true, Style = ButtonStyle.Box)]
            private void RemoveColumn([ValueDropdown("GetAllTableName")] string tableName)
            {
                var tableConfig = TableConfigs.FirstOrDefault(c => c.TableName == tableName);
                TableConfigs.Remove(tableConfig);
            }

            private static IEnumerable<string> GetAllTableName()
            {
                return TableConfigs.Select(c => c.TableName);
            }

#region Table Config Operations
            [PropertySpace(10, 10)]
            [BoxGroup("Table Config")]
            [ButtonGroup("Table Config/Func")]
            [Button("Load Table Config")]
            private void LoadTableConfig()
            {
                var sql = "SELECT name, sql FROM sqlite_master WHERE type='table';";
                var result = DbControl.ExecuteReadonly(sql);
                if (result.Success == false)
                {
                    Debug.LogWarning($"Load Table Config Failed: {result.Message}");
                    return;
                }

                TableConfigs = result.Data.Select(ParseTableConfigFromSql).ToList();

                Debug.Log("Load Table Config Success");
            }

            /// <summary>
            /// Save the table config to the database
            /// <para>It will overwrite the existing table if the table is already exist</para>
            /// <para>And will remove the table if the table is not in the new table configs</para>
            /// </summary>
            [PropertySpace(10, 10)]
            [BoxGroup("Table Config")]
            [ButtonGroup("Table Config/Func")]
            [Button("Save Table Config")]
            private void SaveTableConfig()
            {
                var sql = "SELECT name, sql FROM sqlite_master WHERE type='table';";
                var result = DbControl.ExecuteReadonly(sql);
                if (result.Success == false)
                {
                    Debug.LogError($"Get Table Names Failed: {result.Message}");
                    return;
                }

                var map = new Dictionary<string, (List<Column>?, List<Column>?)>();

                foreach (var tableConfig in TableConfigs)
                {
                    map[tableConfig.TableName] = (tableConfig.Columns, null);
                }

                foreach (var row in result.Data)
                {
                    var config = ParseTableConfigFromSql(row);
                    if (map.TryGetValue(config.TableName, out var columns))
                    {
                        var (value, _) = columns;
                        map[config.TableName] = (value, config.Columns);
                    }
                    else
                    {
                        map[config.TableName] = (null, config.Columns);
                    }
                }

                foreach (var (tableName, (newColumns, existingColumns)) in map)
                {
                    // Drop the old table
                    if (newColumns == null)
                    {
                        var dropTableSql = GenerateDropTableSql(tableName);
                        var dropResult = DbControl.ExecuteUnchecked(dropTableSql);
                        if (dropResult.Success == false)
                        {
                            Debug.LogError($"Drop Table {tableName} Failed: {dropResult.Message}");
                            Debug.LogError(dropTableSql);
                        }
                        continue;
                    }
                    else
                    {
                        // Create the new table
                        if (existingColumns == null)
                        {
                            var createTableSql = GenerateCreateTableSql(new TableConfig
                            {
                                TableName = tableName,
                                Columns = newColumns
                            });
                            var createResult = DbControl.ExecuteUnchecked(createTableSql);
                            if (createResult.Success == false)
                            {
                                Debug.LogError($"Create Table {tableName} Failed: {createResult.Message}");
                                Debug.LogError(createTableSql);
                            }
                            continue;
                        }
                        else
                        {
                            var dropSql = GenerateDropTableSql(tableName);
                            var dropResult = DbControl.ExecuteUnchecked(dropSql);
                            if (dropResult.Success == false)
                            {
                                Debug.LogError($"Drop Table {tableName} Failed: {dropResult.Message}");
                                Debug.LogError(dropSql);
                            }

                            var createTableSql = GenerateCreateTableSql(new TableConfig
                            {
                                TableName = tableName,
                                Columns = newColumns
                            });
                            var createResult = DbControl.ExecuteUnchecked(createTableSql);
                            if (createResult.Success == false)
                            {
                                Debug.LogError($"Create Table {tableName} Failed: {createResult.Message}");
                                Debug.LogError(createTableSql);
                            }
                        }
                    }
                }

                Debug.Log("Save Table Config Success");

                EditorUtility.DisplayDialog("Success", "Save Table Config Success", "OK");
            }
#endregion

#region SQL Generation
            private static string GenerateCreateTableSql(TableConfig tableConfig)
            {
                var sql = $"CREATE TABLE IF NOT EXISTS {tableConfig.TableName} (";
                foreach (var column in tableConfig.Columns)
                {
                    if (column.IsNull())
                    {
                        continue;
                    }

                    sql += $"{column.Name} {column.SqlType.ToPureSqlString()} /* {column.CsType} */,";
                }

                sql = sql.TrimEnd(',') + ");";
                return sql;
            }

            private static TableConfig ParseTableConfigFromSql(Dictionary<string, object> row)
            {
                var reg = new Regex(@"\((.+)\)");
                var match = reg.Match(row["sql"].ToString());
                if (match.Success == false)
                {
                    return default;
                }

                var tableConfig = new TableConfig
                {
                    TableName = row["name"].ToString(),
                    Columns = new List<Column>()
                };

                reg = new Regex(@"(\w+) (\w+) /\* (\w+) \*/");
                var columnMatches = reg.Matches(match.Groups[1].Value);
                foreach (Match columnMatch in columnMatches)
                {
                    var column = new Column
                    {
                        Name = columnMatch.Groups[1].Value,
                        SqlType = columnMatch.Groups[2].Value.ToSqlDataType(),
                        CsType = columnMatch.Groups[3].Value
                    };
                    tableConfig.Columns.Add(column);
                }
                
                return tableConfig;
            }

            private static string GenerateDropTableSql(string tableName)
            {
                return $"DROP TABLE IF EXISTS {tableName};";
            }
#endregion

#region Code Gen

            [ShowInInspector]
            [BoxGroup("Code Gen")]
            public static string CodeGenPath => Path.Combine(ConfigDirPath, "Generate");

            [PropertySpace(10, 10)]
            [Button]
            [BoxGroup("Code Gen")]
            [ButtonGroup("Code Gen/Func")]
            private static void GenerateCodeForAllTable()
            {
                if (Directory.Exists(CodeGenPath) != false)
                {
                    Directory.CreateDirectory(CodeGenPath);
                }
                else
                {
                    Directory.Delete(CodeGenPath, true);
                    Directory.CreateDirectory(CodeGenPath);
                }

                foreach (var config in TableConfigs)
                {
                    var code = GenerateTableConfigStruct(config);
                    var filePath = Path.Combine(CodeGenPath, $"{config.TableName}.g.cs");
                    if (File.Exists(filePath) == false)
                    {
                        File.Create(filePath).Dispose();
                    }
                    File.WriteAllText(filePath, code);

                    var soCode = GenerateTableConfigSO(config);
                    var soFilePath = Path.Combine(CodeGenPath, $"{config.TableName}SO.g.cs");
                    if (File.Exists(soFilePath) == false)
                    {
                        File.Create(soFilePath).Dispose();
                    }
                    File.WriteAllText(soFilePath, soCode);
                }

                CompilationPipeline.RequestScriptCompilation();
            }

            private static string GenerateTableConfigStruct(TableConfig tableConfig)
            {
                var cb = new CodeBuilder();
                var code = cb.AddUsings("System")
                    .WithAccessModifier(CodeBuilder.AccessModifier.Public)
                    .WithStruct(tableConfig.TableName)
                    .AddMembers(tableConfig.Columns, col => new CodeBuilder.Member
                    {
                        Type = CodeBuilder.MemberType.Field,
                        Name = col.Name,
                        TypeOrReturnType = col.CsType,
                        AccessModifier = CodeBuilder.AccessModifier.Public,
                    })
                    .Build();
                
                return code;
            }

            private static string GenerateTableConfigSO(TableConfig tableConfig)
            {
                var cb = new CodeBuilder();
                var code = cb.AddUsings("System", "UnityEngine")
                    .WithAccessModifier(CodeBuilder.AccessModifier.Public)
                    .WithClass($"{tableConfig.TableName}SO")
                    .WithImplements("ScriptableObject")
                    .AddMember(new CodeBuilder.Member
                    {
                        Type = CodeBuilder.MemberType.Field,
                        Name = "Data",
                        TypeOrReturnType = tableConfig.TableName,
                        AccessModifier = CodeBuilder.AccessModifier.Public,
                    })
                    .AddMethod(new CodeBuilder.Method
                    {
                        AccessModifier = CodeBuilder.AccessModifier.Public,
                        Name = "Create"
                    })
                    .Build();

                return string.Empty;
            }

#endregion

        }
    }
}

#endif
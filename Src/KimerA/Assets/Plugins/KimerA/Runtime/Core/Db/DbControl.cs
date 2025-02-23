using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Data;

namespace KimerA.Db
{
    public sealed class DbControl : IDisposable
    {
        private static readonly Lazy<DbControl> instance = new(() => new DbControl());
        private readonly SqliteConnection connection;
        private static readonly Lazy<string> dbPath = new(() => Path.Combine(Application.persistentDataPath, "Data", $"{Application.productName}.db"));
        public static string DbPath => dbPath.Value;

        private DbControl()
        {
            if (Directory.Exists(Path.GetDirectoryName(DbPath)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DbPath));
            }
            if (File.Exists(DbPath) == false)
            {
                SqliteConnection.CreateFile(DbPath);
            }
            connection = new SqliteConnection($"Data Source={DbPath}");
        }

        internal static DbControl Instance => instance.Value;
        
        /// <summary>
        /// Execute a SQL command and return the result
        /// 
        /// <para>This method is only available in the editor</para>
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        internal SqlResult ExecuteUnchecked_Internal(string sql)
        {
            var result = new SqlResult();
            SafeConnection(connection =>
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                try
                {
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        result.Data.Add(row);
                    }
                    result.Success = true;
                    result.Message = "Success";
                }
                catch (Exception e)
                {
                    result.Success = false;
                    result.Message = e.Message;
                }
            });
            return result;
        }

        #if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SqlResult ExecuteUnchecked(string sql)
        {
            return Instance.ExecuteUnchecked_Internal(sql);
        }
        #endif

        public static List<string> GetAllTables()
        {
            var tables = new List<string>();
            SafeConnection(connection =>
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            });
            return tables;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TableExists(string tableName)
        {
            return GetAllTables().Contains(tableName);
        }

        private static void SafeConnection(Action<SafeSqlConnection> action)
        {
            if (Instance.connection.State == ConnectionState.Closed)
            {
                Instance.connection.Open();
                using (var connection = new SafeSqlConnection(Instance.connection))
                {
                    action(connection);
                }
                Instance.connection.Close();
            }
            else if (Instance.connection.State == ConnectionState.Open)
            {
                using (var connection = new SafeSqlConnection(Instance.connection))
                {
                    action(connection);
                }
            }
            else
            {
                Debug.LogWarning($"Not support doing action when connection state is {Instance.connection.State}");
            }
        }

#region SQLite Security
        private static readonly HashSet<string> allowedOpcodes = new()
        {
            "Init", "OpenRead", "Rewind", "Column", "ResultRow", "Next", "Halt", "Transaction", "Goto",
            "Ne", "Lt", "Le", "Gt", "Ge", "Eq", "String8"
        };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SqlResult ExplainSql(string sql)
        {
            var checker = $"EXPLAIN {sql}";
            return ExecuteUnchecked_Internal(checker);
        }

        private bool CheckSqlReadonly(string sql)
        {
            var result = ExplainSql(sql);
            if (result.Success)
            {
                return result.Data.TrueForAll(row => allowedOpcodes.Contains(row["opcode"].ToString()));
            }
            return false;
        }

        public static SqlResult ExecuteReadonly(string sql)
        {
            var lower = sql.ToLower();
            if (lower.StartsWith("explain "))
            {
                return Instance.ExecuteUnchecked_Internal(sql);
            }

            if (Instance.CheckSqlReadonly(sql))
            {
                return Instance.ExecuteUnchecked_Internal(sql);
            }
            return new SqlResult
            {
                Success = false,
                Message = "Not allowed to execute this SQL"
            };
        }
#endregion

        public void Dispose()
        {
            connection?.Close();
            connection?.Dispose();
        }
    }
}
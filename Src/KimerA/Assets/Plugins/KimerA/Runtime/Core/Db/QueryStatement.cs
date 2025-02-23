using System.Collections.Generic;
using System.Text;

namespace KimerA.Db
{
    /// <summary>
    /// Use this to create a sql statement
    /// </summary>
    public class QueryStatement
    {
        private StringBuilder sql = new();
        public List<object> Parameters { get; } = new();

        public QueryStatement Select(string columns)
        {
            sql.Append($"SELECT {columns} ");
            return this;
        }
        
        public QueryStatement From(string table)
        {
            sql.Append($"FROM {table} ");
            return this;
        }

        public QueryStatement Where(string condition, params object[] parameters)
        {
            sql.Append($"WHERE {condition} ");
            Parameters.AddRange(parameters);
            return this;
        }

        public QueryStatement Create(string table, Dictionary<string, string> columns)
        {
            sql.Append($"CREATE TABLE IF NOT EXISTS {table} (");
            foreach (var column in columns)
            {
                sql.Append($"{column.Key} {column.Value}, ");
            }
            sql.Remove(sql.Length - 2, 2);
            sql.Append(");");
            return this;
        }

        public QueryStatement InsertInto(string table, Dictionary<string, object> values)
        {
            sql.Append($"INSERT INTO {table} (");
            foreach (var value in values)
            {
                sql.Append($"{value.Key}, ");
                Parameters.Add(value.Value);
            }
            sql.Remove(sql.Length - 2, 2);
            sql.Append(") VALUES (");
            for (var i = 0; i < values.Count; i++)
            {
                sql.Append($"@{i}, ");
            }
            sql.Remove(sql.Length - 2, 2);
            sql.Append(");");
            return this;
        }

        public QueryStatement Update(string table)
        {
            sql.Append($"UPDATE {table} ");
            return this;
        }

        public QueryStatement Set(string table, Dictionary<string, object> values)
        {
            sql.Append($"UPDATE {table} SET ");
            foreach (var value in values)
            {
                sql.Append($"{value.Key} = @{Parameters.Count}, ");
                Parameters.Add(value.Value);
            }
            sql.Remove(sql.Length - 2, 2);
            return this;
        }

        public QueryStatement DeleteFrom(string table)
        {
            sql.Append($"DELETE FROM {table} ");
            return this;
        }

        public QueryStatement OrderBy(string column, bool desc = false)
        {
            sql.Append($"ORDER BY {column} ");
            if (desc)
            {
                sql.Append("DESC ");
            }
            return this;
        }

        public QueryStatement Limit(int count)
        {
            sql.Append($"LIMIT {count} ");
            return this;
        }

        public QueryStatement Offset(int count)
        {
            sql.Append($"OFFSET {count} ");
            return this;
        }

        public override string ToString()
        {
            return sql.ToString();
        }
    }
}
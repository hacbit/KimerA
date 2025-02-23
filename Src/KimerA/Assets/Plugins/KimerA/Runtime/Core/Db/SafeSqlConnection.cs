using System;
using System.Data;
using Mono.Data.Sqlite;

namespace KimerA.Db
{
    public readonly struct SafeSqlConnection : IDisposable
    {
        private readonly SqliteConnection connection;

        public SafeSqlConnection(SqliteConnection connection)
        {
            this.connection = connection;
        }

        public string ConnectionString => connection.ConnectionString;

        public string DataSource => connection.DataSource;

        public string Database => connection.Database;

        public int DefaultTimeout => connection.DefaultTimeout;

        public string ServerVersion => connection.ServerVersion;

        public ConnectionState State => connection.State;

        public SqliteCommand CreateCommand() => connection.CreateCommand();

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
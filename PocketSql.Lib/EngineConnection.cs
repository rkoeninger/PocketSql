using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using IsolationLevel = System.Data.IsolationLevel;

namespace PocketSql
{
    public class EngineConnection : IDbConnection
    {
        public EngineConnection(Engine engine, SqlVersion sqlVersion)
        {
            this.engine = engine;
            this.sqlVersion = sqlVersion;
        }

        internal readonly Engine engine;
        private readonly SqlVersion sqlVersion;

        private bool open;

        public string Database { get; private set; } = "master";
        public int ConnectionTimeout => 30;

        public string ConnectionString
        {
            get => "";
            set => throw new NotSupportedException("Can't set connection string on PocketSql.EngineConnection");
        }

        public ConnectionState State => open ? ConnectionState.Open : ConnectionState.Closed;
        public void Open() => open = true;
        public void Close() => open = false;
        public void Dispose() => Close();

        public void ChangeDatabase(string databaseName) => Database = databaseName;

        public IDbTransaction BeginTransaction() => BeginTransaction(IsolationLevel.ReadCommitted);
        public IDbTransaction BeginTransaction(IsolationLevel il) => new EngineTransaction(this, il);
        public IDbCommand CreateCommand() => new EngineCommand(this, sqlVersion);
    }
}

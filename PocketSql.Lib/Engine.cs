using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql
{
    public class Engine
    {
        private readonly SqlVersion sqlVersion;

        public Engine(int version) : this(IntToSqlVersion(version)) { }

        private Engine(SqlVersion sqlVersion)
        {
            this.sqlVersion = sqlVersion;
            Databases.Declare(new Database("master"));
        }

        private static SqlVersion IntToSqlVersion(int version) =>
            Enum.TryParse("Sql" + version, out SqlVersion sqlVersion)
                ? sqlVersion
                : throw new NotSupportedException($"SQL Server version {version} not supported");

        public IDbConnection GetConnection() => new EngineConnection(this, sqlVersion);

        public Namespace<Database> Databases { get; } = new Namespace<Database>();
    }
}

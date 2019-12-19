using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql
{
    public class Engine
    {
        private readonly SqlVersion sqlVersion;

        public int Version { get; }

        public Engine(int version) : this(IntToSqlVersion(version)) { }

        private Engine(SqlVersion sqlVersion)
        {
            this.sqlVersion = sqlVersion;
            Version = SqlVersionToInt(sqlVersion);
            Databases.Declare(new Database("master"));
        }

        private static SqlVersion IntToSqlVersion(int version) =>
            Enum.TryParse("Sql" + (version < 40 ? version * 10 : version), out SqlVersion sqlVersion)
                ? sqlVersion
                : throw new NotSupportedException($"SQL Server version {version} not supported");

        private static int SqlVersionToInt(SqlVersion version) =>
            int.Parse(version.ToString().Substring(3, version.ToString().Length - 4));

        public IDbConnection GetConnection() => new EngineConnection(this, sqlVersion);

        public Namespace<Database> Databases { get; } = new Namespace<Database>();
    }
}

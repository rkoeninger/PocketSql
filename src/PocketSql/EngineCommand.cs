using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Evaluation;

namespace PocketSql
{
    public class EngineCommand : IDbCommand
    {
        public EngineCommand(EngineConnection connection, SqlVersion sqlVersion)
        {
            this.connection = connection;
            this.sqlVersion = sqlVersion;
        }

        private EngineConnection connection;
        private readonly SqlVersion sqlVersion;

        public IDbConnection Connection
        {
            get => connection;
            set => connection = value as EngineConnection
                ?? throw new ArgumentException($"{nameof(EngineCommand)} can only use {nameof(EngineConnection)}");
        }

        public IDbTransaction Transaction { get; set; }
        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; } = CommandType.Text;
        public IDataParameterCollection Parameters { get; } = new EngineParameterCollection();
        public UpdateRowSource UpdatedRowSource { get; set; }

        // Command control doesn't do anything
        public void Dispose() { }
        public void Prepare() { }
        public void Cancel() { }

        // TODO: how to set nullability on parameter?
        public IDbDataParameter CreateParameter() => new EngineParameter(true);

        private static bool IsInput(IDataParameter d) =>
            d.Direction == ParameterDirection.Input
            || d.Direction == ParameterDirection.InputOutput;

        private static bool IsOutput(IDataParameter d) =>
            d.Direction == ParameterDirection.InputOutput
            || d.Direction == ParameterDirection.Output;

        private static bool IsReturn(IDataParameter d) =>
            d.Direction == ParameterDirection.ReturnValue;

        private void OnEval(Env env)
        {
            foreach (var param in Parameters.Cast<IDbDataParameter>().Where(IsOutput))
            {
                param.Value = env.Vars[Naming.Parameter(param.ParameterName)];
            }

            foreach (var param in Parameters.Cast<IDbDataParameter>().Where(IsReturn))
            {
                param.Value = env.ReturnValue;
            }
        }

        private List<EngineResult> ExecuteStoredProcedure()
        {
            var env = Env.Of(connection, Parameters);
            var proc = env.Procedures[CommandText];
            var results = Eval.Evaluate(
                proc,
                Parameters
                    .Cast<IDbDataParameter>()
                    .Select(p => (p.ParameterName, IsInput(p), IsOutput(p), p.Value))
                    .ToList(),
                new Scope(env));
            OnEval(env);
            return new List<EngineResult> { results };
        }

        private List<EngineResult> ExecuteText()
        {
            // TODO: you have to create an instance to call the helper?
            var parser = new TSql150Parser(false).Create(sqlVersion, false);
            var input = new StringReader(CommandText);
            var fragment = parser.Parse(input, out var errors);

            if (errors != null && errors.Count > 0)
            {
                throw new Exception(string.Join("\r\n", errors.Select(e => e.Message)));
            }

            var env = Env.Of(connection, Parameters);
            var results = Eval.Evaluate(fragment, new Scope(env));
            OnEval(env);
            return results;
        }

        private List<EngineResult> Execute()
        {
            switch (CommandType)
            {
                case CommandType.Text:
                    return ExecuteText();
                case CommandType.StoredProcedure:
                    return ExecuteStoredProcedure();
                default:
                    throw FeatureNotSupportedException.Value(CommandType);
            }
        }

        public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);
        public IDataReader ExecuteReader(CommandBehavior behavior) => new EngineDataReader(Execute());
        public object ExecuteScalar() => Execute().Last().ResultSet.Rows[0].Values[0];

        public int ExecuteNonQuery()
        {
            var results = Execute();
            return results.Any(x => x.RecordsAffected >= 0)
                ? results.Sum(x => Math.Max(0, x.RecordsAffected))
                : -1;
        }
    }
}

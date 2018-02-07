﻿using System;
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
        public CommandType CommandType { get; set; }
        public IDataParameterCollection Parameters { get; } = new EngineParameterCollection();
        public UpdateRowSource UpdatedRowSource { get; set; }

        // Command control doesn't do anything
        public void Dispose() { }
        public void Prepare() { }
        public void Cancel() { }

        // TODO: how to set nullability on parameter?
        public IDbDataParameter CreateParameter() => new EngineParameter(true);

        private List<EngineResult> Execute()
        {
            // TODO: you have to create an instance to call the helper?
            // TODO: specify SqlEngineType: Azure vs SqlServer?
            var parser = new TSql140Parser(false).Create(sqlVersion, false);
            var input = new StringReader(CommandText);
            var fragment = parser.Parse(input, out var errors);

            if (errors != null && errors.Count > 0)
            {
                throw new Exception(string.Join("\r\n", errors.Select(e => e.Message)));
            }

            var env = Env.Of(connection, Parameters);
            var results = Eval.Evaluate(fragment, env);

            foreach (var param in Parameters.Cast<IDbDataParameter>()
                .Where(x => x.Direction == ParameterDirection.Output
                    || x.Direction == ParameterDirection.InputOutput))
            {
                param.Value = env.Vars[Naming.Parameter(param.ParameterName)];
            }

            foreach (var param in Parameters.Cast<IDbDataParameter>()
                .Where(x => x.Direction == ParameterDirection.ReturnValue))
            {
                param.Value = env.ReturnValue;
            }

            return results;
        }

        public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);
        public IDataReader ExecuteReader(CommandBehavior behavior) => new EngineDataReader(Execute());
        public object ExecuteScalar() => Execute()[0].ResultSet.Rows[0].ItemArray[0];

        public int ExecuteNonQuery()
        {
            var results = Execute();
            return results.Any(x => x.RecordsAffected >= 0)
                ? results.Sum(x => Math.Max(0, x.RecordsAffected))
                : -1;
        }
    }
}
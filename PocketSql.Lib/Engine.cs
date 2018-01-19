using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using IsolationLevel = System.Data.IsolationLevel;

namespace PocketSql
{
    public class Engine
    {
        public IDbConnection GetConnection() => new EngineConnection(this);

        public readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

        public DataSet Evalute(StatementList statements)
        {
            var results = new DataSet();

            foreach (var statment in statements.Statements)
            {
                
            }

            return results;
        }

        public DataTable Evaluate(SelectStatement select)
        {
            var tableRef = (NamedTableReference)((QuerySpecification)select.QueryExpression).FromClause.TableReferences.Single();
            var fullName =
                string.Join(".",
                    tableRef.SchemaObject.ServerIdentifier.Value,
                    tableRef.SchemaObject.DatabaseIdentifier.Value,
                    tableRef.SchemaObject.SchemaIdentifier.Value,
                    tableRef.SchemaObject.BaseIdentifier.Value);
            return tables[fullName];
        }

        public int Evaluate(InsertStatement insert)
        {
            return 0;
        }

        public int Evaluate(UpdateStatement update)
        {
            return 0;
        }

        public int Evaluate(DeleteStatement delete)
        {
            return 0;
        }

        private class EngineConnection : IDbConnection
        {
            public EngineConnection(Engine engine)
            {
                this.engine = engine;
            }

            private readonly Engine engine;

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
            public IDbCommand CreateCommand() => new EngineCommand(this);
        }

        private class EngineTransaction : IDbTransaction
        {
            public EngineTransaction(EngineConnection connection, IsolationLevel il)
            {
                this.connection = connection;
                IsolationLevel = il;
            }

            private readonly EngineConnection connection;

            // Transactions don't do anything
            public void Dispose() { }
            public void Commit() { }
            public void Rollback() { }

            public IDbConnection Connection => connection;
            public IsolationLevel IsolationLevel { get; }
        }

        private class EngineCommand : IDbCommand
        {
            public EngineCommand(EngineConnection connection)
            {
                this.connection = connection;
            }

            private readonly EngineConnection connection;

            public IDbConnection Connection
            {
                get => connection;
                set => throw new NotSupportedException("Can't set Connection on PocketSql.EngineCommand");
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

            public int ExecuteNonQuery()
            {
                throw new NotImplementedException();
            }

            public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);

            public IDataReader ExecuteReader(CommandBehavior behavior)
            {
                throw new NotImplementedException();
            }

            public object ExecuteScalar()
            {
                throw new NotImplementedException();
            }
        }

        private class EngineParameterCollection : IDataParameterCollection
        {
            private readonly List<IDbDataParameter> parameters = new List<IDbDataParameter>();

            public int Add(object value)
            {
                parameters.Add((IDbDataParameter)value);
                return parameters.Count - 1;
            }

            public bool IsReadOnly => false;
            public bool IsFixedSize => false;
            public bool IsSynchronized => false;
            public int Count => parameters.Count;
            public void Clear() => parameters.Clear();
            public bool Contains(object value) => parameters.Contains(value as IDbDataParameter);
            public bool Contains(string parameterName) => parameters.Any(x => x.ParameterName == parameterName);
            public void Insert(int index, object value) => parameters.Insert(index, value as IDbDataParameter);
            public int IndexOf(object value) => parameters.IndexOf(value as IDbDataParameter);
            public int IndexOf(string parameterName) => parameters.FindIndex(x => x.ParameterName == parameterName);
            public void Remove(object value) => parameters.Remove(value as IDbDataParameter);
            public void RemoveAt(int index) => parameters.RemoveAt(index);
            public void RemoveAt(string parameterName) => parameterName.Remove(IndexOf(parameterName));
            public IEnumerator GetEnumerator() => parameters.GetEnumerator();
            public void CopyTo(Array array, int index) => parameters.CopyTo((IDbDataParameter[])array, index);
            public object SyncRoot { get; } = new object();

            object IList.this[int index]
            {
                get => parameters[index];
                set => parameters[index] = (IDbDataParameter) value;
            }

            object IDataParameterCollection.this[string parameterName]
            {
                get => parameters.First(x => x.ParameterName == parameterName);
                set => parameters[IndexOf(parameterName)] = (IDbDataParameter) value;
            }
        }

        private class EngineParameter : IDbDataParameter
        {
            public EngineParameter(bool nullable)
            {
                IsNullable = nullable;
            }

            public DbType DbType { get; set; }
            public ParameterDirection Direction { get; set; }
            public bool IsNullable { get; }
            public string ParameterName { get; set; }
            public string SourceColumn { get; set; }
            public DataRowVersion SourceVersion { get; set; }
            public object Value { get; set; }
            public byte Precision { get; set; }
            public byte Scale { get; set; }
            public int Size { get; set; }
        }
    }
}

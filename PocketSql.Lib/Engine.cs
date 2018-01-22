using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using IsolationLevel = System.Data.IsolationLevel;

namespace PocketSql
{
    public class Engine
    {
        public IDbConnection GetConnection() => new EngineConnection(this);

        public readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

        public DataSet Evaluate(StatementList statements)
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

            internal readonly Engine engine;

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

            private DataSet Execute()
            {
                var parser = new TSql140Parser(false, SqlEngineType.Standalone);
                var input = new StringReader(CommandText);
                var statements = parser.ParseStatementList(input, out var errors);
                // TODO: raise parse errors
                return connection.engine.Evaluate(statements);
            }

            public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);

            public IDataReader ExecuteReader(CommandBehavior behavior)
            {
                return new EngineDataReader(Execute());
            }

            public int ExecuteNonQuery()
            {
                Execute();
                return -1; // TODO: get lines affected - Evaluate could return broader data type
                // Defaults to -1 if lines affected is somehow unknown
            }

            public object ExecuteScalar()
            {
                Execute();
                return null; // TODO: how to interpret scalar from dataset?
                             //       see how ADO does it
            }
        }

        private class EngineDataReader : IDataReader
        {
            private readonly DataSet data;
            private int tableIndex = -1;
            private int rowIndex = -1;

            public EngineDataReader(DataSet data)
            {
                this.data = data;
            }

            public bool IsClosed { get; private set; } = false;
            public void Close() => IsClosed = true;
            public void Dispose() => Close();
            public object this[int i] => GetValue(i);
            public object this[string name] => this[GetOrdinal(name)];

            public int Depth => throw new NotImplementedException();
            public int RecordsAffected => throw new NotImplementedException();
            public int FieldCount => data.Tables[tableIndex].Columns.Count;

            public DataTable GetSchemaTable() => throw new NotImplementedException();

            public string GetName(int i) => data.Tables[tableIndex].Columns[i].ColumnName;
            public int GetOrdinal(string name) => data.Tables[tableIndex].Columns[name].Ordinal;

            public Type GetFieldType(int i) => data.Tables[tableIndex].Columns[i].DataType;
            public string GetDataTypeName(int i) => GetFieldType(i).Name;

            public bool IsDBNull(int i) => data.Tables[tableIndex].Rows[rowIndex].IsNull(i);

            public bool GetBoolean(int i) => (bool) GetValue(i);
            public byte GetByte(int i) => (byte) GetValue(i);
            public DateTime GetDateTime(int i) => (DateTime) GetValue(i);
            public decimal GetDecimal(int i) => (decimal) GetValue(i);
            public double GetDouble(int i) => (double) GetValue(i);
            public float GetFloat(int i) => (float) GetValue(i);
            public Guid GetGuid(int i) => (Guid) GetValue(i);
            public short GetInt16(int i) => (short) GetValue(i);
            public int GetInt32(int i) => (int) GetValue(i);
            public long GetInt64(int i) => (long) GetValue(i);
            public string GetString(int i) => (string) GetValue(i);
            public char GetChar(int i) => (char)GetValue(i);
            public object GetValue(int i) => data.Tables[tableIndex].Rows[rowIndex].ItemArray[i];

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
            {
                var bytes = (byte[]) GetValue(i);
                var amount =
                    Math.Max(0,
                        Math.Min(length,
                            Math.Min(buffer.Length - bufferOffset, bytes.Length - fieldOffset)));
                Array.Copy(bytes, fieldOffset, buffer, bufferOffset, amount);
                return amount;
            }

            public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
            {
                var chars = (char[]) GetValue(i);
                var amount =
                    Math.Max(0,
                        Math.Min(length,
                            Math.Min(buffer.Length - bufferOffset, chars.Length - fieldOffset)));
                Array.Copy(chars, fieldOffset, buffer, bufferOffset, amount);
                return amount;
            }

            public int GetValues(object[] values)
            {
                var i = 0;

                for (; i < values.Length && i < FieldCount; ++i)
                {
                    values[i] = GetValue(i);
                }

                return i;
            }

            public IDataReader GetData(int i) => throw new NotImplementedException();

            public bool NextResult()
            {
                tableIndex++;
                rowIndex = -1;
                return tableIndex < data.Tables.Count;
            }

            public bool Read()
            {
                rowIndex++;
                return rowIndex < data.Tables[tableIndex].Rows.Count;
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

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
        private readonly SqlVersion sqlVersion;

        public Engine(int version) : this(IntToSqlVersion(version)) { }

        // TODO: make public
        private Engine(SqlVersion sqlVersion)
        {
            this.sqlVersion = sqlVersion;
        }

        private static SqlVersion IntToSqlVersion(int version)
        {
            switch (version)
            {
                case 80:  return SqlVersion.Sql80;
                case 90:  return SqlVersion.Sql90;
                case 100: return SqlVersion.Sql100;
                case 110: return SqlVersion.Sql110;
                case 120: return SqlVersion.Sql120;
                case 130: return SqlVersion.Sql130;
                case 140: return SqlVersion.Sql140;
            }

            throw new NotSupportedException($"SQL Server version {version} not supported");
        }

        public IDbConnection GetConnection() => new EngineConnection(this, sqlVersion);

        private readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

        private List<EngineResult> Evaluate(StatementList statements, IDictionary<string, object> vars) =>
            statements.Statements.Select(s => Evaluate(s, vars)).ToList();

        private EngineResult Evaluate(TSqlStatement statement, IDictionary<string, object> vars)
        {
            switch (statement)
            {
                case SelectStatement select:
                    return Evaluate(select, vars);
                case UpdateStatement update:
                    return Evaluate(update, vars);
                case InsertStatement insert:
                    return Evaluate(insert, vars);
                case DeleteStatement delete:
                    return Evaluate(delete, vars);
                case CreateTableStatement createTable:
                    return Evaluate(createTable);
                case SetVariableStatement set:
                    return Evaluate(set, vars);
                default:
                    throw new NotImplementedException();
            }
        }

        private EngineResult Evaluate(SelectStatement select, IDictionary<string, object> vars)
        {
            var querySpec = (QuerySpecification) select.QueryExpression;
            var tableRef = (NamedTableReference) querySpec.FromClause.TableReferences.Single();
            var table = tables[tableRef.SchemaObject.BaseIdentifier.Value];
            var projection = new DataTable();
            
            var selections = querySpec.SelectElements.SelectMany(s =>
            {
                switch (s)
                {
                    case SelectStarExpression star:
                        return table.Columns.Cast<DataColumn>().Select(c =>
                            (c.ColumnName,
                                c.DataType,
                                (ScalarExpression)CreateColumnReferenceExpression(c.ColumnName)));
                    case SelectScalarExpression scalar:
                        return new []
                        {
                            (scalar.ColumnName?.Value ?? InferName(scalar.Expression),
                                InferType(scalar.Expression, table),
                                scalar.Expression)
                        }.AsEnumerable();
                    default:
                        throw new NotImplementedException();
                }
            }).ToList();
            
            foreach (var (name, type, _) in selections)
            {
                projection.Columns.Add(new DataColumn
                {
                    ColumnName = name,
                    DataType = type
                });
            }

            foreach (DataRow row in table.Rows)
            {
                if (querySpec.WhereClause == null || Evaluate(querySpec.WhereClause.SearchCondition, row, vars))
                {
                    var resultRow = projection.NewRow();

                    for (var i = 0; i < selections.Count; ++i)
                    {
                        resultRow[i] = Evaluate(selections[i].Item3, row, vars);
                    }

                    projection.Rows.Add(resultRow);
                }
            }

            return new EngineResult
            {
                ResultSet = projection
            };
        }

        private EngineResult Evaluate(InsertStatement insert, IDictionary<string, object> vars)
        {
            var tableRef = (NamedTableReference) insert.InsertSpecification.Target;
            var table = tables[tableRef.SchemaObject.BaseIdentifier.Value];
            var columnOrder = insert.InsertSpecification.Columns;
            var valuesExprs = ((ValuesInsertSource) insert.InsertSpecification.InsertSource).RowValues;

            foreach (var valuesExpr in valuesExprs)
            {
                var row = table.NewRow();

                for (var i = 0; i < columnOrder.Count; ++i)
                {
                    row[columnOrder[i].MultiPartIdentifier.Identifiers[0].Value] =
                        Evaluate(valuesExpr.ColumnValues[i], null, vars);
                }

                table.Rows.Add(row);
            }

            return new EngineResult
            {
                RecordsAffected = valuesExprs.Count
            };
        }

        private EngineResult Evaluate(UpdateStatement update, IDictionary<string, object> vars)
        {
            var tableRef = (NamedTableReference)update.UpdateSpecification.Target;
            var table = tables[tableRef.SchemaObject.BaseIdentifier.Value];
            var rowCount = 0;

            foreach (DataRow row in table.Rows)
            {
                if (update.UpdateSpecification.WhereClause == null
                    || Evaluate(update.UpdateSpecification.WhereClause.SearchCondition, row, vars))
                {
                    foreach (var clause in update.UpdateSpecification.SetClauses)
                    {
                        switch (clause)
                        {
                            case AssignmentSetClause set:
                                var columnName = set.Column.MultiPartIdentifier.Identifiers.Last().Value;
                                row[columnName] = Evaluate(
                                    set.AssignmentKind,
                                    row[columnName],
                                    Evaluate(set.NewValue, row, vars));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    rowCount++;
                }
            }

            return new EngineResult
            {
                RecordsAffected = rowCount
            };
        }

        private ColumnReferenceExpression CreateColumnReferenceExpression(string name)
        {
            var schemaObjectName = new SchemaObjectName();
            schemaObjectName.Identifiers.Add(new Identifier());
            schemaObjectName.Identifiers.Add(new Identifier());
            schemaObjectName.Identifiers.Add(new Identifier());
            schemaObjectName.Identifiers.Add(new Identifier());
            schemaObjectName.BaseIdentifier.Value = name;

            // TODO: qualify rest of name

            return new ColumnReferenceExpression
            {
                MultiPartIdentifier = schemaObjectName
            };
        }

        private string InferName(ScalarExpression expr)
        {
            switch (expr)
            {
                case ColumnReferenceExpression colRefExpr:
                    return colRefExpr.MultiPartIdentifier.Identifiers.Last().Value;
                case VariableReference varRef:
                    return varRef.Name;
            }

            return null;
        }

        private Type InferType(ScalarExpression expr, DataTable table)
        {
            // TODO: a lot of work to do here for type inference
            //       how does sql server do it?
            //       is it just the lowest common type between all values in a column?
            //       do the columns not have type?

            switch (expr)
            {
                // TODO: need to handle multi-table disambiguation
                case ColumnReferenceExpression colRefExpr:
                    var name = colRefExpr.MultiPartIdentifier.Identifiers.Last().Value;
                    return table.Columns[name].DataType;
            }

            throw new NotImplementedException();
        }

        private EngineResult Evaluate(DeleteStatement delete, IDictionary<string, object> vars)
        {
            var tableRef = (NamedTableReference) delete.DeleteSpecification.Target;
            var table = tables[tableRef.SchemaObject.BaseIdentifier.Value];
            var rowCount = 0;

            foreach (DataRow row in table.Rows)
            {
                if (delete.DeleteSpecification.WhereClause == null
                    || Evaluate(delete.DeleteSpecification.WhereClause.SearchCondition, row, vars))
                {
                    table.Rows.Remove(row);
                    rowCount++;
                }
            }

            return new EngineResult
            {
                RecordsAffected = rowCount
            };
        }

        private EngineResult Evaluate(CreateTableStatement createTable)
        {
            var table = new DataTable();

            foreach (var column in createTable.Definition.ColumnDefinitions)
            {
                table.Columns.Add(new DataColumn
                {
                    ColumnName = column.ColumnIdentifier.Value,
                    DataType = TranslateType(column.DataType),
                    //MaxLength = 
                    //AllowDbNull = 
                });
            }

            tables.Add(createTable.SchemaObjectName.BaseIdentifier.Value, table);
            return new EngineResult();
        }

        private EngineResult Evaluate(SetVariableStatement set, IDictionary<string, object> vars)
        {
            vars[set.Variable.Name] = Evaluate(set.Expression, null, vars);
            return new EngineResult();
        }

        private object Evaluate(AssignmentKind kind, object current, object value)
        {
            switch (kind)
            {
                case AssignmentKind.Equals:
                    return value;
                case AssignmentKind.AddEquals:
                    // TODO: handle numeric conversions properly
                    if (current is int && value is int)
                        return (int) current + (int) value;
                    return (decimal)current + (decimal)value;
                case AssignmentKind.SubtractEquals:
                    return (decimal)current - (decimal)value;
                case AssignmentKind.MultiplyEquals:
                    return (decimal)current * (decimal)value;
                case AssignmentKind.DivideEquals:
                    return (decimal)current / (decimal)value;
                case AssignmentKind.ModEquals:
                    return (decimal)current % (decimal)value;
                case AssignmentKind.BitwiseAndEquals:
                    return (int)current & (int)value;
                case AssignmentKind.BitwiseOrEquals:
                    return (int)current | (int)value;
                case AssignmentKind.BitwiseXorEquals:
                    return (int)current ^ (int)value;
            }

            throw new NotImplementedException();
        }

        private bool Evaluate(BooleanExpression expr, DataRow row, IDictionary<string, object> vars)
        {
            switch (expr)
            {
                case BooleanBinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, row, vars),
                        Evaluate(binaryExpr.SecondExpression, row, vars));
                case BooleanComparisonExpression compareExpr:
                    return Evaluate(
                        compareExpr.ComparisonType,
                        Evaluate(compareExpr.FirstExpression, row, vars),
                        Evaluate(compareExpr.SecondExpression, row, vars));
                case BooleanNotExpression notExpr:
                    return !Evaluate(notExpr.Expression, row, vars);
                case BooleanIsNullExpression isNullExpr:
                    return Evaluate(isNullExpr.Expression, row, vars) == null;
                case InPredicate inExpr:
                    var value = Evaluate(inExpr.Expression, row, vars);
                    return value != null && inExpr.Values.Any(x => value.Equals(Evaluate(x, row, vars)));
            }

            throw new NotImplementedException();
        }

        private object Evaluate(ScalarExpression expr, DataRow row, IDictionary<string, object> vars)
        {
            switch (expr)
            {
                case IntegerLiteral intLiteral:
                    return int.Parse(intLiteral.Value);
                case NumericLiteral numericExpr:
                    return decimal.Parse(numericExpr.Value);
                case StringLiteral stringExpr:
                    return stringExpr.Value;
                case UnaryExpression unaryExpr:
                    return Evaluate(
                        unaryExpr.UnaryExpressionType,
                        Evaluate(unaryExpr.Expression, row, vars));
                case BinaryExpression binaryExpr:
                    return Evaluate(
                        binaryExpr.BinaryExpressionType,
                        Evaluate(binaryExpr.FirstExpression, row, vars),
                        Evaluate(binaryExpr.SecondExpression, row, vars));
                case ColumnReferenceExpression colExpr:
                    return row[colExpr.MultiPartIdentifier.Identifiers.Last().Value];
                case VariableReference varRef:
                    return vars[varRef.Name.TrimStart('@')];
            }

            throw new NotImplementedException();
        }

        private object Evaluate(UnaryExpressionType op, object value)
        {
            switch (op)
            {
                case UnaryExpressionType.Positive:
                    return value;
                case UnaryExpressionType.Negative:
                    return -1 * (decimal) value;
                case UnaryExpressionType.BitwiseNot:
                    return ~ (int) value;
            }

            throw new NotImplementedException();
        }

        private object Evaluate(BinaryExpressionType op, object left, object right)
        {
            switch (op)
            {
                case BinaryExpressionType.Add:
                    if (left is string && right is string)
                        return (string) left + (string) right;
                    return (decimal)left + (decimal)right;
                case BinaryExpressionType.Subtract:
                    return (decimal)left - (decimal)right;
                case BinaryExpressionType.Multiply:
                    return (decimal)left * (decimal)right;
                case BinaryExpressionType.Divide:
                    return (decimal)left / (decimal)right;
                case BinaryExpressionType.Modulo:
                    return (decimal)left % (decimal)right;
                case BinaryExpressionType.BitwiseAnd:
                    return (int)left & (int)right;
                case BinaryExpressionType.BitwiseOr:
                    return (int)left & (int)right;
                case BinaryExpressionType.BitwiseXor:
                    return (int)left ^ (int)right;
            }

            throw new NotImplementedException();
        }

        private bool Evaluate(BooleanBinaryExpressionType op, bool left, bool right)
        {
            // TODO: emulate sql server short-circuit behavior (may be version specific)
            switch (op)
            {
                case BooleanBinaryExpressionType.And:
                    return left && right;
                case BooleanBinaryExpressionType.Or:
                    return left || right;
            }

            throw new NotImplementedException();
        }

        private bool Evaluate(BooleanComparisonType op, object left, object right)
        {
            switch (op)
            {
                case BooleanComparisonType.Equals:
                    return left != null && left.Equals(right);
                case BooleanComparisonType.NotEqualToBrackets:
                case BooleanComparisonType.NotEqualToExclamation:
                    return left == null || !left.Equals(right);
                case BooleanComparisonType.GreaterThan:
                    return (decimal) left > (decimal) right;
                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return (decimal) left >= (decimal) right;
                case BooleanComparisonType.LessThan:
                    return (decimal)left < (decimal)right;
                case BooleanComparisonType.LessThanOrEqualTo:
                    return (decimal)left <= (decimal)right;
            }

            throw new NotImplementedException();
        }

        private static Type TranslateType(DataTypeReference typeRef)
        {
            if (typeRef is SqlDataTypeReference type)
            {
                switch (type.SqlDataTypeOption)
                {
                    case SqlDataTypeOption.Bit:
                        return typeof(bool);
                    case SqlDataTypeOption.TinyInt:
                        return typeof(sbyte);
                    case SqlDataTypeOption.SmallInt:
                        return typeof(short);
                    case SqlDataTypeOption.Int:
                        return typeof(int);
                    case SqlDataTypeOption.BigInt:
                        return typeof(long);
                    case SqlDataTypeOption.Float:
                        return typeof(float);
                    case SqlDataTypeOption.Decimal:
                        return typeof(decimal);
                    case SqlDataTypeOption.DateTime:
                        return typeof(DateTime);
                    case SqlDataTypeOption.NText:
                    case SqlDataTypeOption.NVarChar:
                    case SqlDataTypeOption.Text:
                    case SqlDataTypeOption.VarChar:
                        return typeof(string);
                }
            }

            throw new NotImplementedException();
        }

        private class EngineConnection : IDbConnection
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
            public EngineCommand(EngineConnection connection, SqlVersion sqlVersion)
            {
                this.connection = connection;
                this.sqlVersion = sqlVersion;
            }

            private readonly EngineConnection connection;
            private readonly SqlVersion sqlVersion;

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

            private List<EngineResult> Execute()
            {
                // TODO: you have to create an instance to call the helper?
                // TODO: specify SqlEngineType: Azure vs SqlServer?
                var parser = new TSql140Parser(false).Create(sqlVersion, false);
                var input = new StringReader(CommandText);
                var statements = parser.ParseStatementList(input, out var errors);
                // TODO: raise parse errors
                return connection.engine.Evaluate(
                    statements,
                    Parameters.Cast<IDbDataParameter>().ToDictionary(x => x.ParameterName, x => x.Value));
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

        private class EngineResult
        {
            public int RecordsAffected { get; set; } = -1;
            public DataTable ResultSet { get; set; }
        }

        private class EngineDataReader : IDataReader
        {
            private readonly List<EngineResult> data;
            private int tableIndex;
            private int rowIndex = -1;

            public EngineDataReader(List<EngineResult> data)
            {
                this.data = data;
            }

            public bool IsClosed { get; private set; }
            public void Close() => IsClosed = true;
            public void Dispose() => Close();
            public object this[int i] => GetValue(i);
            public object this[string name] => this[GetOrdinal(name)];

            public int Depth => 0;
            public int RecordsAffected => data[tableIndex].RecordsAffected;
            public int FieldCount => data[tableIndex].ResultSet.Columns.Count;

            public DataTable GetSchemaTable() => throw new NotImplementedException();

            public string GetName(int i) => data[tableIndex].ResultSet.Columns[i].ColumnName;
            public int GetOrdinal(string name) => data[tableIndex].ResultSet.Columns[name].Ordinal;

            public Type GetFieldType(int i) => data[tableIndex].ResultSet.Columns[i].DataType;
            public string GetDataTypeName(int i) => GetFieldType(i).Name;

            public bool IsDBNull(int i) => data[tableIndex].ResultSet.Rows[rowIndex].IsNull(i);

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
            public object GetValue(int i) => data[tableIndex].ResultSet.Rows[rowIndex].ItemArray[i];

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
                return tableIndex < data.Count;
            }

            public bool Read()
            {
                rowIndex++;
                return rowIndex < data[tableIndex].ResultSet.Rows.Count;
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

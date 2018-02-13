using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PocketSql.Modeling
{
    // TODO: columns and rows can be owned and point back to their parents
    // TODO: ...or they might not be owned
    // TODO: or they might have no name and only an ordinal
    // TODO: need a class just for a table cell/passable reference?

    public class BaseColumn
    {
        public string Name { get; set; }
        public DbType DbType { get; set; }
        public bool Nullable { get; set; }
        public Maybe<int> Size { get; set; }

        // TODO: need to consider implicit conversions
        public bool IsValid(object value)
        {
            if (value == null || value == DBNull.Value) return Nullable;

            switch (DbType)
            {
                case DbType.Single:
                    return value is float;
                case DbType.Double:
                    return value is double;
                case DbType.Decimal:
                    return value is decimal;
                case DbType.Byte:
                    return value is byte;
                case DbType.UInt16:
                    return value is ushort;
                case DbType.UInt32:
                    return value is uint;
                case DbType.UInt64:
                    return value is ulong;
                case DbType.SByte:
                    return value is sbyte;
                case DbType.Int16:
                    return value is short;
                case DbType.Int32:
                    return value is int;
                case DbType.Int64:
                    return value is long;
                case DbType.Boolean:
                    return value is bool;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return value is DateTime;
                case DbType.Guid:
                    return value is Guid;
                case DbType.Time:
                    return value is TimeSpan;
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return Maybe.Some(value)
                        .OfType<string>()
                        .Select(v => Size.Select(s => v.Length <= s).OrElse(true))
                        .OrElse(false);
                case DbType.Object:
                    return true;
                case DbType.Currency:
                case DbType.VarNumeric:
                case DbType.Xml:
                default:
                    throw FeatureNotSupportedException.Value(DbType);
            }
        }
    }

    public interface IHeading
    {
        IList<BaseColumn> Columns { get; set; }
    }

    public interface ITable
    {
        string Database { get; set; }
        string Schema { get; set; }
        string Name { get; set; }
        IHeading Heading { get; set; }
        IList<IRow> Rows { get; }
    }

    public interface IRow
    {
        object this[string database, string schema, string table, string column] { get; set; }
    }

    public class Heading : IHeading
    {
        public IList<BaseColumn> Columns { get; set; }
    }

    public class BaseTable : ITable
    {
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public IHeading Heading { get; set; }
        public IList<IRow> Rows { get; } = new List<IRow>();
    }

    public class BaseRow : IRow
    {
        public IHeading Heading { get; set; }
        public object[] Items { get; set; }

        private int GetColumnIndex(string column) =>
            Enumerable.Range(0, Heading.Columns.Count).First(x => column.Similar(Heading.Columns[x].Name));

        public object this[string database, string schema, string table, string column]
        {
            get => Items[GetColumnIndex(column)];
            set => Items[GetColumnIndex(column)] = value;
        }
    }

    public class JoinedHeading : IHeading
    {
        public IList<IHeading> Headings { get; set; }

        public IList<BaseColumn> Columns
        {
            get => Headings.SelectMany(x => x.Columns).ToList();
            set => throw new NotSupportedException();
        }
    }

    public class JoinedTable : ITable
    {
        public string Database { get; set; } // TODO: has no meaning?
        public string Schema { get; set; } // TODO: has no meaning?
        public string Name { get; set; }
        public IHeading Heading { get; set; }
        public IList<JoinedRow> JoinedRows { get; } = new List<JoinedRow>();
        public IList<IRow> Rows => JoinedRows.Cast<IRow>().ToList();
    }

    public class JoinedRow : IRow
    {
        public JoinedHeading Heading { get; set; }
        public List<(string, string, string, IRow)> Rows { get; set; }

        private IRow GetRow(string database, string schema, string table) =>
            Rows.First(x => x.Item1.Similar(database) && x.Item2.Similar(schema) && x.Item3.Similar(table)).Item4;

        public object this[string database, string schema, string table, string column]
        {
            get => GetRow(database, schema, table)[database, schema, table, column];
            set => GetRow(database, schema, table)[database, schema, table, column] = value;
        }
    }

    public interface IArgument { }

    public class RowArgument : IArgument
    {
        public RowArgument(DataRow value)
        {
            Value = value;
        }

        public DataRow Value { get; set; }
    }

    public class GroupArgument : IArgument
    {
        public GroupArgument(EquatableList key, List<DataRow> rows)
        {
            Key = key;
            Rows = rows;
        }

        public EquatableList Key { get; set; }
        public List<DataRow> Rows { get; set; }
    }

    public class NullArgument : IArgument
    {
        public static NullArgument It = new NullArgument();
    }
}

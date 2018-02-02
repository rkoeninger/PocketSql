using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PocketSql
{
    // TODO: columns and rows can be owned and point back to their parents
    // TODO: ...or they might not be owned
    // TODO: or they might have no name and only an ordinal
    // TODO: need a class just for a table cell/passable reference?

    public class Column
    {
        public string Name { get; set; }
        public DbType DbType { get; set; }
        public Type CsType { get; set; }
        public bool Nullable { get; set; }
    }

    public interface IHeading
    {
        IList<Column> Columns { get; set; }
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
        public IList<Column> Columns { get; set; }
    }

    public class Table : ITable
    {
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public IHeading Heading { get; set; }
        public IList<IRow> Rows { get; } = new List<IRow>();
    }

    public class Row : IRow
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

        public IList<Column> Columns
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
        public DataRow Value { get; set; }
    }

    public class GroupArgument : IArgument
    {
        public EquatableList Key { get; set; }
        public List<DataRow> Rows { get; set; }
    }

    public class ScalarArgument : IArgument
    {
        public object Value { get; }

        public ScalarArgument(object value)
        {
            Value = value;
        }
    }

    public class NullArgument : IArgument { }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public interface IOutputSink
    {
        void Inserted(Row row, Env env);
        void Updated(Row oldRow, Row newRow, Env env);
        void Deleted(Row row, Env env);
    }

    public class TableOutputSink : IOutputSink
    {
        public Table Output { get; }

        public TableOutputSink(IList<Column> columns) => Output = new Table { Columns = columns };

        public void Inserted(Row row, Env env)
        {
            var outputRow = Output.NewRow(env);

            foreach (var i in Enumerable.Range(0, Output.Columns.Count))
            {
                var columnName = Output.Columns[i].Name.LastOrDefault();
                var prefix = Output.Columns[i].Name.FirstOrDefault();

                if (columnName == null)
                {
                    throw new Exception("Column name cannot be null");
                }

                if (columnName.Similar("$action"))
                {
                    outputRow.Values[i] = "INSERT";
                    continue;
                }

                if (prefix == null)
                {
                    throw new Exception("column name in OUTPUT must have INSERTED or DELETED prefix");
                }

                if (prefix.Similar("inserted"))
                {
                    outputRow.SetValue(columnName, row.GetValue(columnName));
                }
                else if (prefix.Similar("deleted"))
                {
                    outputRow.SetValue(columnName, null);
                }
            }
        }

        public void Updated(Row oldRow, Row newRow, Env env)
        {
            var outputRow = Output.NewRow(env);

            foreach (var i in Enumerable.Range(0, Output.Columns.Count))
            {
                var columnName = Output.Columns[i].Name.LastOrDefault();
                var prefix = Output.Columns[i].Name.FirstOrDefault();

                if (columnName == null)
                {
                    throw new Exception("Column name cannot be null");
                }

                if (columnName.Similar("$action"))
                {
                    outputRow.Values[i] = "UPDATE";
                    continue;
                }

                if (prefix == null)
                {
                    throw new Exception("column name in OUTPUT must have INSERTED or DELETED prefix");
                }

                if (prefix.Similar("inserted"))
                {
                    outputRow.SetValue(columnName, newRow.GetValue(columnName));
                }
                else if (prefix.Similar("deleted"))
                {
                    outputRow.SetValue(columnName, oldRow.GetValue(columnName));
                }
            }
        }

        public void Deleted(Row row, Env env)
        {
            var outputRow = Output.NewRow(env);

            foreach (var i in Enumerable.Range(0, Output.Columns.Count))
            {
                var columnName = Output.Columns[i].Name.LastOrDefault();
                var prefix = Output.Columns[i].Name.FirstOrDefault();

                if (columnName == null)
                {
                    throw new Exception("Column name cannot be null");
                }

                if (columnName.Similar("$action"))
                {
                    outputRow.Values[i] = "DELETE";
                    continue;
                }

                if (prefix == null)
                {
                    throw new Exception("column name in OUTPUT must have INSERTED or DELETED prefix");
                }

                if (prefix.Similar("inserted"))
                {
                    outputRow.SetValue(columnName, null);
                }
                else if (prefix.Similar("deleted"))
                {
                    outputRow.SetValue(columnName, row.GetValue(columnName));
                }
            }
        }
    }

    public class NullOutputSink : IOutputSink
    {
        public void Inserted(Row row, Env env) { }
        public void Updated(Row oldRow, Row newRow, Env env) { }
        public void Deleted(Row row, Env env) { }
    }
}

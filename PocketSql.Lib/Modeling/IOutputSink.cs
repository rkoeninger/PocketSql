using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSql.Modeling
{
    public interface IOutputSink
    {
        void Inserted(Row row);
        void Updated(Row oldRow, Row newRow);
        void Deleted(Row row);
    }

    public class TableOutputSink : IOutputSink
    {
        public Table Output { get; }

        public TableOutputSink(IList<Column> columns)
        {
            Output = new Table { Columns = columns };
        }

        public void Inserted(Row row)
        {
            var outputRow = Output.NewRow();

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

        public void Updated(Row oldRow, Row newRow)
        {
            var outputRow = Output.NewRow();

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

        public void Deleted(Row row)
        {
            var outputRow = Output.NewRow();

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
        public void Inserted(Row row)
        {
        }

        public void Updated(Row oldRow, Row newRow)
        {
        }

        public void Deleted(Row row)
        {
        }
    }
}

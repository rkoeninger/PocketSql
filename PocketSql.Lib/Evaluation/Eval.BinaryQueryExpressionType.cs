using System;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(BinaryQueryExpressionType type, bool all, DataTable first, DataTable right)
        {
            // TODO: what to do when columns don't match?
            // TODO: does offset/fetch/top happen before or after?

            switch (type)
            {
                case BinaryQueryExpressionType.Except:
                    // The SQL EXCEPT operator takes the distinct rows of one query and
                    // returns the rows that do not appear in a second result set. The
                    // EXCEPT ALL operator does not remove duplicates. For purposes of
                    // row elimination and duplicate removal, the EXCEPT operator does
                    // not distinguish between NULLs.
                    break;
                case BinaryQueryExpressionType.Intersect:
                    // The SQL INTERSECT operator takes the results of two queries and
                    // returns only rows that appear in both result sets.For purposes of
                    // duplicate removal the INTERSECT operator does not distinguish between
                    // NULLs.The INTERSECT operator removes duplicate rows from the final
                    // result set. The INTERSECT ALL operator does not remove duplicate rows
                    // from the final result set.
                    break;
                case BinaryQueryExpressionType.Union:
                    // In SQL the UNION clause combines the results of two SQL queries into
                    // a single table of all matching rows. The two queries must result in
                    // the same number of columns and compatible data types in order to unite.
                    // Any duplicate records are automatically removed unless UNION ALL is used.

                    // UNION can be useful in data warehouse applications where tables aren't
                    // perfectly normalized. A simple example would be a database having
                    // tables sales2005 and sales2006 that have identical structures but are
                    // separated because of performance considerations. A UNION query could
                    // combine results from both tables.

                    // Note that UNION does not guarantee the order of rows. Rows from the
                    // second operand may appear before, after, or mixed with rows from the
                    // first operand.In situations where a specific order is desired, ORDER BY
                    // must be used.

                    // Note that UNION ALL may be much faster than plain UNION.
                    break;
            }

            throw new NotImplementedException();
        }
    }
}

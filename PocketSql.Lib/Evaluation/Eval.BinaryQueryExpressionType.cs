using System;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using PocketSql.Modeling;

namespace PocketSql.Evaluation
{
    public static partial class Eval
    {
        public static EngineResult Evaluate(BinaryQueryExpressionType type, bool all, Table left, Table right)
        {
            // TODO: does offset/fetch/top happen before or after?

            if (left.Columns.Count != right.Columns.Count)
            {
                throw new Exception("tables must have the same number of columns");
            }

            foreach (var i in Enumerable.Range(0, left.Columns.Count))
            {
                var a = left.Columns[i];
                var b = right.Columns[i];

                if (a.Name.Length > 0 && b.Name.Length > 0 && !a.Name.Last().Similar(b.Name.Last()))
                {
                    throw new Exception("columns must have the same names");
                }

                // TODO: identify lowest common type
                if (a.Type != b.Type)
                {
                    throw new Exception("types must match");
                }
            }

            var result = new Table { Columns = left.Columns };

            bool Contains(Table t, Row r) =>
                t.Rows.Any(s =>
                    Enumerable.Range(0, r.Columns.Count).All(i =>
                        Equality.Equal(r.Values[i], s.Values[i])));

            switch (type)
            {
                case BinaryQueryExpressionType.Except:
                    foreach (var x in left.Rows)
                    {
                        if (!Contains(right, x) && (all || !Contains(result, x))) result.AddCopy(x);
                    }

                    break;
                case BinaryQueryExpressionType.Intersect:
                    foreach (var x in left.Rows)
                    {
                        if (Contains(right, x) && (all || !Contains(result, x))) result.AddCopy(x);
                    }

                    if (all)
                    {
                        foreach (var x in right.Rows)
                        {
                            if (Contains(left, x)) result.AddCopy(x);
                        }
                    }

                    break;
                case BinaryQueryExpressionType.Union:
                    foreach (var x in left.Rows)
                    {
                        if (all || !Contains(result, x)) result.AddCopy(x);
                    }

                    foreach (var x in right.Rows)
                    {
                        if (all || !Contains(result, x)) result.AddCopy(x);
                    }

                    break;
                default:
                    throw FeatureNotSupportedException.Value(type);
            }

            return new EngineResult(result);
        }
    }
}

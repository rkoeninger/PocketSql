using System;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql
{
    public static class ParseIt
    {
        public static void LogTokens(string sql)
        {
            var parser = new TSql140Parser(false);
            var input = new StringReader(sql);
            var fragments = parser.Parse(input, out var errors);

            if (errors.Count > 0)
            {
                throw new Exception(string.Join("\r\n", errors));
            }

            foreach (var token in fragments.ScriptTokenStream.Where(x => x.TokenType != TSqlTokenType.WhiteSpace))
            {
                if (token.TokenType != TSqlTokenType.WhiteSpace
                    && token.TokenType != TSqlTokenType.EndOfFile)
                {
                    Console.WriteLine($"{token.TokenType} @ ({token.Line}, {token.Column}): \"{token.Text}\"");
                }
            }
        }

        public static void LogStatements(string sql)
        {
            var parser = new TSql140Parser(false, SqlEngineType.Standalone);
            var input = new StringReader(sql);
            var statements = parser.ParseStatementList(input, out var errors);
            
            if (errors.Count > 0)
            {
                throw new Exception(string.Join("\r\n", errors));
            }

            foreach (var statement in statements.Statements)
            {
                Console.WriteLine(statement.GetType().Name);

                if (statement is SelectStatement select)
                {
                    Console.WriteLine($@"
                        select {string.Join(", ", select.ComputeClauses)}
                        into {select.Into?.BaseIdentifier}
                        from {select.On?.Value}
                        where {select.QueryExpression?.ForClause}");
                }

                if (statement is InsertStatement insert)
                {
                    Console.WriteLine($@"
                        insert {insert.InsertSpecification.InsertOption} {(insert.InsertSpecification.Target as NamedTableReference)?.SchemaObject?.BaseIdentifier.Value}
                        ({string.Join(", ", insert.InsertSpecification.Columns.Select(x => x.MultiPartIdentifier.Identifiers[0].Value))})
                        values {string.Join(", ",
                            (insert.InsertSpecification.InsertSource as ValuesInsertSource)?.RowValues[0]?
                                .ColumnValues?
                                .Select(ScalarToString) ?? new string[0])}");
                }

                Console.WriteLine();
            }
        }

        private static string ScalarToString(ScalarExpression expr)
        {
            if (expr is StringLiteral s)
            {
                return $"'{s.Value}'";
            }

            if (expr is IntegerLiteral i)
            {
                return i.Value;
            }

            return "other-expr";
        }
    }
}

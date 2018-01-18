using System;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql
{
    public static class ParseIt
    {
        public static void Main(string sql)
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
    }
}

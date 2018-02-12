using System;
using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace PocketSql.Modeling
{
    public class Procedure : INamed
    {
        public string Name { get; set; }
        public IDictionary<string, Type> Parameters { get; set; }
        public StatementList Statements { get; set; }
    }
}

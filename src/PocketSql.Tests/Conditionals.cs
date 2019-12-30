using System.Data;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Conditionals
    {
        [Test]
        public void CaseExpression([All]IDbConnection connection)
        {
            Assert.AreEqual(4, connection.QueryFirst<int>(@"
                select (case 'a' when 'a' then 4 else 3 end)"));
            Assert.AreEqual(3, connection.QueryFirst<int>(@"
                select (case 'a' when 'b' then 4 else 3 end)"));
            Assert.AreEqual(4, connection.QueryFirst<int>(@"
                select (case when 1 = 1 then 4 else 3 end)"));
            Assert.AreEqual(3, connection.QueryFirst<int>(@"
                select (case when 1 = 0 then 4 else 3 end)"));
        }

        [Test]
        public void IifExpression([AsOf(11)]IDbConnection connection)
        {
            Assert.AreEqual(4, connection.QueryFirst<int>(@"
                select iif('a' = 'a', 4, 3)"));
            Assert.AreEqual(3, connection.QueryFirst<int>(@"
                select iif('a' = 'b', 4, 3)"));
        }

        [Test]
        public void IfStatement([All]IDbConnection connection)
        {
            Assert.AreEqual(1, connection.ExecuteScalar<int>("if 1 = 1 select 1"));
        }

        [Test]
        public void IfElseStatement([All]IDbConnection connection)
        {
            Assert.AreEqual(2, connection.ExecuteScalar<int>("if 1 = 2 select 1 else select 2"));
        }
    }
}

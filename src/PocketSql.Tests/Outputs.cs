using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Outputs
    {
        [Test]
        public void InsertOutput([AsOf(10)]IDbConnection connection)
        {
            connection.Execute(@"
                create table Things
                (
                    X int,
                    Y varchar(8)
                )");
            var results = connection.Query<int>(@"
                insert into Things
                (X, Y)
                output
                    inserted.X
                values
                (1, 'abc'),
                (2, 'def'),
                (3, 'ghi')");
            Assert.IsTrue(new[] { 1, 2, 3 }.SequenceEqual(results));
        }

        [Test]
        public void UpdateOutput([AsOf(10)]IDbConnection connection)
        {
            connection.Execute(@"
                create table Things
                (
                    X int,
                    Y varchar(8)
                )");
            connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (1, 'abc'),
                (2, 'def'),
                (3, 'ghi')");
            var results = connection.Query<Thing>(@"
                update Things
                set
                    X += 5,
                    Y = upper(Y)
                output inserted.X, deleted.Y");
            Assert.IsTrue(new[]
            {
                new Thing(6, "abc"),
                new Thing(7, "def"),
                new Thing(8, "ghi")
            }.SequenceEqual(results));
        }
    }
}

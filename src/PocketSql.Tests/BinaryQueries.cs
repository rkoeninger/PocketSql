using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class BinaryQueries
    {
        private static void SetupBinaryQueryEngine(IDbConnection connection)
        {
            connection.Execute(@"
                create table AAA
                (
                    X int,
                    Y varchar(8)
                )");
            connection.Execute(@"
                create table BBB
                (
                    X int,
                    Y varchar(8)
                )");
            connection.Execute(@"
                insert into AAA
                (X, Y)
                values
                (1, 'abc'),
                (1, 'abc'),
                (3, 'ghi'),
                (4, 'jkl'),
                (4, 'jkl')");
            connection.Execute(@"
                insert into BBB
                (X, Y)
                values
                (1, 'abc'),
                (2, 'def'),
                (3, 'ghi'),
                (5, 'mno'),
                (5, 'mno')");
        }

        [Test]
        public void Union([AsOf(10)]IDbConnection connection)
        {
            SetupBinaryQueryEngine(connection);
            var results = connection.Query<Thing>(@"
                (select * from AAA)
                union
                (select * from BBB)").ToList();
            Assert.AreEqual(5, results.Count);
        }

        [Test]
        public void UnionAll([AsOf(10)]IDbConnection connection)
        {
            SetupBinaryQueryEngine(connection);
            var results = connection.Query<Thing>(@"
                (select * from AAA)
                union all
                (select * from BBB)").ToList();
            Assert.AreEqual(10, results.Count);
        }

        [Test]
        public void Intersect([AsOf(10)]IDbConnection connection)
        {
            SetupBinaryQueryEngine(connection);
            var results = connection.Query<Thing>(@"
                (select * from AAA)
                intersect
                (select * from BBB)").ToList();
            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void IntersectAll([AsOf(10)]IDbConnection connection)
        {
            SetupBinaryQueryEngine(connection);
            var results = connection.Query<Thing>(@"
                (select * from AAA)
                intersect all
                (select * from BBB)").ToList();
            Assert.AreEqual(5, results.Count);
        }

        [Test]
        public void Except([AsOf(10)]IDbConnection connection)
        {
            SetupBinaryQueryEngine(connection);
            var results = connection.Query<Thing>(@"
                (select * from AAA)
                except
                (select * from BBB)").ToList();
            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void ExceptReversed([AsOf(10)]IDbConnection connection)
        {
            SetupBinaryQueryEngine(connection);
            var results = connection.Query<Thing>(@"
                (select * from BBB)
                except
                (select * from AAA)").ToList();
            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void ExceptAll([AsOf(10)]IDbConnection connection)
        {
            SetupBinaryQueryEngine(connection);
            var results = connection.Query<Thing>(@"
                    (select * from AAA)
                    except all
                    (select * from BBB)").ToList();
            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void ExceptAllReversed([AsOf(10)]IDbConnection connection)
        {
            SetupBinaryQueryEngine(connection);
            var results = connection.Query<Thing>(@"
                    (select * from BBB)
                    except all
                    (select * from AAA)").ToList();
            Assert.AreEqual(3, results.Count);
        }
    }
}

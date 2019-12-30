using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Selects
    {
        [Test]
        public void SelectOrderBy([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(16, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (34, 'qwe'),
                (23, 'wer'),
                (67, 'ert'),
                (63, 'rty'),
                (75, 'tyu'),
                (17, 'yui'),
                (83, 'uio'),
                (47, 'iop'),
                (95, 'asd'),
                (36, 'sdf'),
                (67, 'dfg'),
                (23, 'fgh'),
                (50, 'ghj'),
                (17, 'hjk'),
                (95, 'jkl'),
                (92, 'zxc')"));
            var things = connection.Query<Thing>(@"
                select X, Y
                from Things
                order by X, Y desc");
            Assert.IsTrue(new[]
            {
                new Thing(17, "yui"),
                new Thing(17, "hjk"),
                new Thing(23, "wer"),
                new Thing(23, "fgh"),
                new Thing(34, "qwe"),
                new Thing(36, "sdf"),
                new Thing(47, "iop"),
                new Thing(50, "ghj"),
                new Thing(63, "rty"),
                new Thing(67, "ert"),
                new Thing(67, "dfg"),
                new Thing(75, "tyu"),
                new Thing(83, "uio"),
                new Thing(92, "zxc"),
                new Thing(95, "jkl"),
                new Thing(95, "asd")
            }.SequenceEqual(things));
        }

        [Test]
        public void SelectOffsetFetch([AsOf(11)]IDbConnection connection)
        {
            connection.Execute("create table Numbers (X int)");
            Assert.AreEqual(8, connection.Execute(@"
                insert into Numbers (X)
                values (1), (2), (3), (4), (5), (6), (7), (8)"));
            Assert.IsTrue(new int?[] { 3, 4, 5, 6 }.SequenceEqual(connection.Query<int?>(@"
                select X
                from Numbers
                order by X
                offset 2 rows
                fetch next 4 rows only")));
        }

        [Test]
        public void SelectDistinct([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(16, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (34, 'qwe'),
                (23, 'wer'),
                (67, 'ert'),
                (63, 'rty'),
                (75, 'tyu'),
                (17, 'yui'),
                (47, 'uio'),
                (47, 'zxc'),
                (95, 'asd'),
                (67, 'ert'),
                (63, 'rty'),
                (75, 'tyu'),
                (95, 'asd'),
                (17, 'yui'),
                (23, 'wer'),
                (92, 'zxc')"));
            Assert.AreEqual(10, connection.Query("select distinct * from Things").Count());
        }

        [Test]
        public void SelectInto([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(16, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (34, 'qwe'),
                (23, 'wer'),
                (67, 'ert'),
                (63, 'rty'),
                (75, 'tyu'),
                (17, 'yui'),
                (47, 'uio'),
                (47, 'zxc'),
                (95, 'asd'),
                (67, 'ert'),
                (63, 'rty'),
                (75, 'tyu'),
                (95, 'asd'),
                (17, 'yui'),
                (23, 'wer'),
                (92, 'zxc')"));
            Assert.AreEqual(16, connection.Execute("select * into Others from Things"));
            Assert.AreEqual(16, connection.Query("select * from Others").Count());
        }

        [Test]
        public void SelectExpression([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Numbers (X int)");
            Assert.AreEqual(8, connection.Execute(@"
                insert into Numbers (X)
                values (1), (2), (3), (4), (5), (6), (7), (8)"));
            Assert.IsTrue(
                new[] { 2, 3, 4, 5, 6, 7, 8, 9 }.SequenceEqual(
                    connection.Query<int>("select X + 1 from Numbers")));
        }

        [Test]
        public void SelectGroupBy([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(16, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (3, 'a'),
                (2, 'b'),
                (6, 'c'),
                (6, 'a'),
                (7, 'b'),
                (1, 'a'),
                (4, 'c'),
                (4, 'd'),
                (9, 'b'),
                (6, 'a'),
                (6, 'b'),
                (7, 'd'),
                (9, 'c'),
                (1, 'a'),
                (2, 'b'),
                (9, 'd')"));
            var result = connection.Query<Thing>(@"
                select sum(X) as X, Y
                from Things
                group by Y").ToList();
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(17, result.First(t => t.Y == "a").X);
            Assert.AreEqual(26, result.First(t => t.Y == "b").X);
            Assert.AreEqual(19, result.First(t => t.Y == "c").X);
            Assert.AreEqual(20, result.First(t => t.Y == "d").X);
        }

        [Test]
        public void SelectGroupByHaving([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(16, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (3, 'a'),
                (2, 'b'),
                (6, 'c'),
                (6, 'a'),
                (7, 'b'),
                (1, 'a'),
                (4, 'c'),
                (4, 'd'),
                (9, 'b'),
                (6, 'a'),
                (6, 'b'),
                (7, 'd'),
                (9, 'c'),
                (1, 'a'),
                (2, 'b'),
                (9, 'd')"));
            var result = connection.Query<Thing>(@"
                select sum(X) as X, Y
                from Things
                group by Y
                having sum(X) >= 20").ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(26, result.First(t => t.Y == "b").X);
            Assert.AreEqual(20, result.First(t => t.Y == "d").X);
        }

        [Test]
        public void SelectCte([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y int)");
            Assert.AreEqual(16, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (34, 12),
                (23, 54),
                (67, 83),
                (63, 26),
                (75, 94),
                (17, 25),
                (47, 56),
                (47, 25),
                (95, 83),
                (67, 46),
                (63, 97),
                (75, 38),
                (95, 85),
                (17, 35),
                (23, 84),
                (92, 32)"));
            Assert.AreEqual(4, connection.Query(@"
                ; with Temp as
                (
                    select Y
                    from Things
                    where X > 50
                )
                select Y
                from Temp
                where Y < 50"
            ).Count());
        }

        [Test]
        public void SelectView([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(16, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (34, 'qwe'),
                (23, 'wer'),
                (67, 'ert'),
                (63, 'rty'),
                (75, 'tyu'),
                (17, 'yui'),
                (47, 'uio'),
                (47, 'zxc'),
                (95, 'asd'),
                (67, 'ert'),
                (63, 'rty'),
                (75, 'tyu'),
                (95, 'asd'),
                (17, 'yui'),
                (23, 'wer'),
                (92, 'zxc')"));
            connection.Execute(@"
                create view FilteredThings
                as
                    select *
                    from Things
                    where X < 30;");
            Assert.AreEqual(4, connection.Query("select * from FilteredThings").Count());
        }
    }
}

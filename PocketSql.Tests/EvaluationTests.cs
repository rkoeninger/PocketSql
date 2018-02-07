using System;
using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    [TestFixture]
    public class EvaluationTests
    {
        [Test]
        public void CreateInsertSelect()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table People (Name varchar(32), Age int)");

                Assert.AreEqual(1, connection.Execute("insert into People (Name, Age) values ('Rob', 30)"));

                var people = connection.Query<Person>("select Name, Age from People").ToList();
                Assert.AreEqual(1, people.Count);
                Assert.AreEqual("Rob", people[0].Name);
                Assert.AreEqual(30, people[0].Age);

                Assert.AreEqual(1, connection.Execute("update People set Age += 1 where Name = @Name", new { Name = "Rob" }));

                people = connection.Query<Person>("select Name, Age from People where Name = 'Rob'").ToList();
                Assert.AreEqual(1, people.Count);
                Assert.AreEqual("Rob", people[0].Name);
                Assert.AreEqual(31, people[0].Age);
            }
        }

        [Test]
        public void InsertSelectOrdered()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
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

                Assert.IsTrue(new []
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
                }.SequenceEqual(connection.Query<Thing>("select X, Y from Things order by X, Y desc")));
            }
        }

        public class Thing : IEquatable<Thing>
        {
            public Thing()
            {
            }

            public Thing(int x, string y)
            {
                X = x;
                Y = y;
            }

            public int X { get; set; }
            public string Y { get; set; }

            public override bool Equals(object that) => that is Thing && Equals((Thing) that);

            public override int GetHashCode()
            {
                unchecked
                {
                    // ReSharper disable NonReadonlyMemberInGetHashCode
                    return (X * 397) ^ (Y != null ? Y.GetHashCode() : 0);
                    // ReSharper enable NonReadonlyMemberInGetHashCode
                }
            }

            public bool Equals(Thing that) => that != null && X == that.X && Y == that.Y;
        }

        [Test]
        public void InsertSelectOffsetFetch()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table Numbers (X int)");

                Assert.AreEqual(8, connection.Execute(@"
                    insert into Numbers (X)
                    values (1), (2), (3), (4), (5), (6), (7), (8)"));

                Assert.IsTrue(new int?[] {3, 4, 5, 6}.SequenceEqual(connection.Query<int?>(@"
                    select X
                    from Numbers
                    order by X
                    offset 2 rows
                    fetch next 4 rows only")));
            }
        }

        [Test]
        public void InsertSelectDistinct()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
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
        }

        [Test]
        public void InsertSelectIntoSelect()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
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
        }

        [Test]
        public void InsertSelect()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table Things (X int, Y varchar(8))");
                connection.Execute("create table Others (X int)");

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

                Assert.AreEqual(16, connection.Execute("insert into Others select X from Things"));

                Assert.AreEqual(16, connection.Query("select * from Others").Count());
            }
        }

        [Test]
        public void InsertSelectTransform()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                connection.Execute("create table Numbers (X int)");

                Assert.AreEqual(8, connection.Execute(@"
                    insert into Numbers (X)
                    values (1), (2), (3), (4), (5), (6), (7), (8)"));

                Assert.IsTrue(
                    new [] {2, 3, 4, 5, 6, 7, 8, 9}.SequenceEqual(
                        connection.Query<int>("select X + 1 from Numbers")));
            }
        }

        [Test]
        public void InsertSelectGroupBy()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
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

                var result = connection.Query<Thing>("select sum(X) as X, Y from Things group by Y").ToList();
                Assert.AreEqual(4, result.Count);
                Assert.AreEqual(17, result.First(t => t.Y == "a").X);
                Assert.AreEqual(26, result.First(t => t.Y == "b").X);
                Assert.AreEqual(19, result.First(t => t.Y == "c").X);
                Assert.AreEqual(20, result.First(t => t.Y == "d").X);
            }
        }

        [Test]
        public void InsertSelectGroupByHaving()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
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

                var result = connection.Query<Thing>("select sum(X) as X, Y from Things group by Y having sum(X) >= 20").ToList();
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual(26, result.First(t => t.Y == "b").X);
                Assert.AreEqual(20, result.First(t => t.Y == "d").X);
            }
        }

        [Test]
        public void StoredProc()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
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
                    create procedure DoIt
                        @Limit int
                    as
                        select * from Things where X > @Limit");

                Assert.AreEqual(
                    9,
                    connection.Query(
                        "DoIt",
                        new { Limit = 50 },
                        commandType: CommandType.StoredProcedure).Count());

                connection.Execute(@"
                    alter procedure DoIt
                        @Limit int
                    as
                        select * from Things where X < @Limit");

                Assert.AreEqual(
                    5,
                    connection.Query(
                        "DoIt",
                        new { Limit = 40 },
                        commandType: CommandType.StoredProcedure).Count());
            }
        }

        [Test]
        public void DeclareSetSelect()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                Assert.AreEqual("Rob", connection.ExecuteScalar<string>(@"
                    declare @Name varchar(16)
                    set @Name = 'Rob'
                    select @Name"));
            }
        }

        [Test, Ignore("StoredProcedure command needs to scope parameters directly onto proc called")]
        public void OutputParameter()
        {
            var engine = new Engine(140);

            using (var connection = engine.GetConnection())
            {
                var p = new DynamicParameters();
                p.Add("@a", 11);
                p.Add("@b", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@c", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
                connection.Execute(@"
                    create proc TestProc
                        @a int,
                        @b int output
                    as
                    begin
                        set @b = 999
                        select 1111
                        return @a
                    end");
                Assert.AreEqual(1111, connection.QueryFirst<int>(
                    "TestProc",
                    p,
                    commandType: CommandType.StoredProcedure));
                Assert.AreEqual(11, p.Get<int>("@c"));
                Assert.AreEqual(999, p.Get<int>("@b"));
            }
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}

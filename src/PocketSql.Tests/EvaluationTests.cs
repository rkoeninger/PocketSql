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
        public void UpdatePlusEquals([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table People (Name varchar(32), Age int)");
            Assert.AreEqual(1, connection.Execute("insert into People (Name, Age) values ('Rob', 30)"));
            var people = connection.Query<Person>("select p.Name, p.Age from People as p").ToList();
            Assert.AreEqual(1, people.Count);
            Assert.AreEqual("Rob", people[0].Name);
            Assert.AreEqual(30, people[0].Age);
            Assert.AreEqual(1, connection.Execute("update People set Age += 1 where Name = @Name", new { Name = "Rob" }));
            people = connection.Query<Person>("select Name, Age from People where Name = 'Rob'").ToList();
            Assert.AreEqual(1, people.Count);
            Assert.AreEqual("Rob", people[0].Name);
            Assert.AreEqual(31, people[0].Age);
        }

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

        public class Thing : IEquatable<Thing>
        {
            public Thing() { }

            public Thing(int x, string y)
            {
                X = x;
                Y = y;
            }

            public int X { get; set; }
            public string Y { get; set; }

            public override bool Equals(object that) => that is Thing thing && Equals(thing);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X * 397) ^ (Y != null ? Y.GetHashCode() : 0);
                }
            }

            public bool Equals(Thing that) => that != null && X == that.X && Y == that.Y;
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
        public void SelectIsNull([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8) null)");
            Assert.AreEqual(4, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (34, 'qwe'),
                (23, null),
                (67, 'ert'),
                (63, null)"));
            var things = connection.Query<Thing>("select isnull(Y, 'missing') as Y, X from Things");
            Assert.IsTrue(new[] {
                new Thing(34, "qwe"),
                new Thing(23, "missing"),
                new Thing(67, "ert"),
                new Thing(63, "missing")
            }.SequenceEqual(things));
        }

        [Test]
        public void SelectNullIf([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8) null)");
            Assert.AreEqual(4, connection.Execute(@"
                insert into Things
                (X, Y)
                values
                (34, 'qwe'),
                (23, 'asd'),
                (67, 'qwe'),
                (63, 'ert')"));
            var things = connection.Query<Thing>("select nullif(Y, 'qwe') as Y, X from Things");
            Assert.IsTrue(new[]{
                new Thing(34, null),
                new Thing(23, "asd"),
                new Thing(67, null),
                new Thing(63, "ert")
            }.SequenceEqual(things));
        }

        [Test]
        public void InsertSelect([AsOf(10)]IDbConnection connection)
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

        [Test]
        public void CreateAndInsertAndSelectCustomSchema([AsOf(8)]IDbConnection connection)
        {
            connection.Execute("create table abc.Things (X int, Y varchar(8))");
            connection.Execute("insert into abc.Things (X, Y) values (1, 'abc')");
            var thing = connection.QueryFirstOrDefault<Thing>("select * from abc.Things");
            Assert.AreEqual(new Thing(1, "abc"), thing);
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
        public void MergeTables([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Source (SourceId int, SourceAmount int)");
            Assert.AreEqual(4, connection.Execute(@"
                insert into Source
                (SourceId, SourceAmount)
                values
                (1, 20),
                (2, 50),
                (3, 30),
                (4, 90)"));
            connection.Execute("create table Target (TargetId int, TargetAmount int)");
            Assert.AreEqual(4, connection.Execute(@"
                insert into Target
                (TargetId, TargetAmount)
                values
                (1, 30),
                (3, 10),
                (5, 100),
                (6, 80)"));
            connection.Execute(@"
                merge Target as t
                using Source as s
                on TargetId = SourceId
                when matched then
                    update set
                        TargetAmount += SourceAmount
                when not matched then
                    insert (TargetId, TargetAmount)
                    values (SourceId, SourceAmount);");
            int GetTargetAmount(int id) =>
                connection.QueryFirstOrDefault<int>(@"
                    select TargetAmount
                    from Target
                    where TargetId = @id", new { id });
            Assert.AreEqual(50, GetTargetAmount(1));
            Assert.AreEqual(90, GetTargetAmount(4));
            Assert.AreEqual(80, GetTargetAmount(6));
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
        public void StoredProc([AsOf(10)]IDbConnection connection)
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

        [Test]
        public void BuiltInFunction([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (Y varchar(8))");
            Assert.AreEqual(16, connection.Execute(@"
                insert into Things
                (Y)
                values
                ('  qwe'),
                ('wer '),
                ('  ert '),
                (' rty  '),
                ('tyu  '),
                (' yui'),
                (' uio '),
                ('zxc'),
                ('asd'),
                (' ert  '),
                ('rty '),
                (' tyu'),
                ('  asd'),
                ('yui '),
                ('   wer '),
                ('  zxc ')"));
            Assert.IsTrue(connection
                .Query<string>("select trim(Y) from Things")
                .All(x => x.Length == 3));
        }

        [Test]
        public void UserDefinedFunction([AsOf(9)]IDbConnection connection)
        {
            connection.Execute(@"
                create function ScalarMax(@X int, @Y int)
                returns int
                as
                begin
                    return (case when @X > @Y then @X else @Y end);
                end;");
            Assert.AreEqual(5, connection.QueryFirst<int>("select ScalarMax(3, 5)"));
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

        [Test]
        public void DeclareSetSelect([AsOf(8)]IDbConnection connection)
        {
            Assert.AreEqual("Rob", connection.ExecuteScalar<string>(@"
                declare @Name varchar(16)
                set @Name = 'Rob'
                select @Name"));
        }

        [Test]
        public void OutputParameter([AsOf(8)]IDbConnection connection)
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

        [Test]
        public void CursorFetchNext([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(8, connection.Execute(@"
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
                (47, 'zxc')"));
            var result = connection.ExecuteScalar<int>(@"
                declare @total int = 0
                declare cur cursor for select * from Things
                open cur

                fetch from cur into @X, @Y

                while @@fetch_status = 0
                begin
                    set @total += @X
                    fetch from cur into @X, @Y
                end

                close cur
                deallocate cur
                select @total");
            Assert.AreEqual(339, result);
        }

        [Test]
        public void SelectRowCountGlobal([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(8, connection.ExecuteScalar<int>(@"
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
                (47, 'zxc');

                select @@rowcount;"));
        }

        [Test]
        public void CaseExpression([AsOf(8)]IDbConnection connection)
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

        private static void CreateEngineWithCityColorTaste(IDbConnection connection)
        {
            connection.Execute(@"
                create table TableA (X int, City varchar(32))

                insert into TableA
                (X, City)
                values
                (1, 'atlanta'),
                (2, 'boston'),
                (3, 'chicago'),
                (4, 'dallas'),
                (5, 'erie'),
                (6, 'frankfort'),
                (7, 'geneva'),
                (8, 'hamburg'),
                (9, 'indianapolis'),
                (10, 'jakarta'),
                (11, 'knoxville'),
                (12, 'leipzig')

                create table TableB (X int, Y int, Color varchar(12))

                insert into TableB
                (X, Y, Color)
                values
                (1, 26, 'red'),
                (2, 25, 'blue'),
                (4, 25, 'green'),
                (5, 24, 'yellow'),
                (7, 22, 'purple'),
                (8, 22, 'orange'),
                (10, 21, 'purple'),
                (12, 21, 'gray')

                create table TableC (Y int, Taste varchar(8))

                insert into TableC
                (Y, Taste)
                values
                (21, 'sweet'),
                (22, 'spicy'),
                (23, 'sour'),
                (24, 'bitter'),
                (25, 'savory'),
                (26, 'salty')");
        }

        [Test]
        public void InnerJoin([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select a.City, b.Color, c.Taste
                from TableA as a
                join TableB as b on a.X = b.X
                join TableC as c on b.Y = c.Y").ToList();
            Assert.AreEqual(8, results.Count);
        }

        [Test]
        public void OuterJoin([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select b.Color, c.Taste
                from TableB as b
                full outer join TableC as c on b.Y = c.Y").ToList();
            Assert.AreEqual(9, results.Count);
        }

        [Test]
        public void CrossJoin([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select *
                from TableA as a
                cross join TableB as b").ToList();
            Assert.AreEqual(96, results.Count);
        }

        [Test]
        public void CrossApply([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select * from TableA as a
                cross apply
                (select * from TableB as b) as c").ToList();
            Assert.AreEqual(96, results.Count);
        }

        [Test]
        public void OuterApply([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select * from TableA as a
                outer apply
                (select * from TableB as b) as c").ToList();
            Assert.AreEqual(96, results.Count);
        }

        public class CityColorTaste
        {
            public string City { get; set; }
            public string Color { get; set; }
            public string Taste { get; set; }
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Test]
        public void DateAdd([AsOf(8)]IDbConnection connection)
        {
            DateTime Run(string datepart, int x) =>
                connection.ExecuteScalar<DateTime>(
                    $"select dateadd({datepart}, {x}, '2018-01-01')");
            Assert.AreEqual(new DateTime(2018, 1, 2), Run("day", 1));
            Assert.AreEqual(new DateTime(2017, 2, 1), Run("month", -11));
            Assert.AreEqual(new DateTime(2018, 1, 1, 7, 0, 0), Run("hour", 7));
        }

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

        [Test]
        public void IdentityColumn([AsOf(8)]IDbConnection connection)
        {
            connection.Execute(@"
                create table Things
                (
                    X int identity,
                    Y varchar(8)
                )");
            connection.Execute(@"
                insert into Things (Y) values ('abc')
                insert into Things (Y) values ('def')
                insert into Things (Y) values ('ghi')");
            var things = connection.Query<Thing>(@"select * from Things");
            Assert.IsTrue(new[]
            {
                new Thing(1, "abc"),
                new Thing(2, "def"),
                new Thing(3, "ghi")
            }.SequenceEqual(things));
        }

        [Test]
        public void IdentitySeedIncrementColumn([AsOf(8)]IDbConnection connection)
        {
            connection.Execute(@"
                create table Things
                (
                    X int identity(100, 10),
                    Y varchar(8)
                )");
            connection.Execute(@"
                insert into Things (Y) values ('abc')
                insert into Things (Y) values ('def')
                insert into Things (Y) values ('ghi')");
            var things = connection.Query<Thing>(@"select * from Things");
            Assert.IsTrue(new[]
            {
                new Thing(100, "abc"),
                new Thing(110, "def"),
                new Thing(120, "ghi")
            }.SequenceEqual(things));
        }

        [Test]
        public void DefaultColumn([AsOf(8)]IDbConnection connection)
        {
            connection.Execute(@"
                create table Things
                (
                    X int,
                    Y varchar(8) default('abc')
                )");
            connection.Execute(@"
                insert into Things (X) values (1)
                insert into Things (X) values (2)
                insert into Things (X) values (3)");
            var things = connection.Query<Thing>(@"select * from Things");
            Assert.IsTrue(new[]
            {
                new Thing(1, "abc"),
                new Thing(2, "abc"),
                new Thing(3, "abc")
            }.SequenceEqual(things));
        }

        [Test]
        public void InsertNull([AsOf(8)]IDbConnection connection)
        {
            connection.Execute(@"
                create table Things
                (
                    X int,
                    Y varchar(8) null
                )");
            connection.Execute(@"
                insert into Things (X, Y) values (0, null)");
            var things = connection.Query<Thing>(@"select * from Things");
            Assert.IsTrue(new[] { new Thing(0, null) }.SequenceEqual(things));
        }
    }
}

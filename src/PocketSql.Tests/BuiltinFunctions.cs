using System;
using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class BuiltinFunctions
    {
        [Test]
        public void DateAdd([All]IDbConnection connection)
        {
            DateTime Run(string datepart, int x) =>
                connection.ExecuteScalar<DateTime>(
                    $"select dateadd({datepart}, {x}, '2018-01-01')");
            Assert.AreEqual(new DateTime(2018, 1, 2), Run("day", 1));
            Assert.AreEqual(new DateTime(2017, 2, 1), Run("month", -11));
            Assert.AreEqual(new DateTime(2018, 1, 1, 7, 0, 0), Run("hour", 7));
        }

        [Test]
        public void NewId([All]IDbConnection connection)
        {
            Assert.IsInstanceOf<Guid>(connection.ExecuteScalar("select newid()"));
        }

        [Test]
        public void IsNumeric([All]IDbConnection connection)
        {
            Assert.AreEqual(1, connection.ExecuteScalar<int>("select isnumeric(1)"));
            Assert.AreEqual(1, connection.ExecuteScalar<int>("select isnumeric(1.0)"));
            Assert.AreEqual(0, connection.ExecuteScalar<int>("select isnumeric('1')"));
        }

        [Test]
        public void Coalesce([All]IDbConnection connection)
        {
            Assert.AreEqual(2, connection.ExecuteScalar<int>("select coalesce(null, 2)"));
            Assert.AreEqual(3, connection.ExecuteScalar<int>("select coalesce(null, null, 3)"));
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
        public void TestIsNullFunction([AsOf(9)]IDbConnection connection)
        {
            connection.Execute(SampleTable);
            var ids = connection.Query<IdAndName>(@"
                SELECT Id, IsNull(Name, 'NO NAME GIVEN') AS 'Name'
                FROM [Sample]
                ORDER BY Name")?.ToList();
            Assert.IsNotNull(ids);
            Assert.AreEqual(8, ids?.Count);
            Assert.AreEqual(1, ids?.Count(x => x.Name == "NO NAME GIVEN"));
        }

        [Test]
        public void TestNullIfFunction([AsOf(9)]IDbConnection connection)
        {
            connection.Execute(SampleTable);
            var ids = connection.Query<IdAndName>(@"
                SELECT Id, NullIf(Name, 'Gwen') AS 'Name'
                FROM [Sample]
                ORDER BY Name")?.ToList();
            Assert.IsNotNull(ids);
            Assert.AreEqual(8, ids?.Count);
            Assert.AreEqual(2, ids?.Count(x => string.IsNullOrEmpty(x.Name)));
            Assert.IsFalse(ids?.Any(x => x.Name == "Gwen"));
        }

        private const string SampleTable = @"
            CREATE TABLE [Sample]
            (
                [Id] [int] IDENTITY(1000, 1) NOT FOR REPLICATION NOT NULL,
                [Name] [varchar](80) NOT NULL
            );
      
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 116725, 'Alison');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 216726, 'Barbara');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 316727, 'Chrisine');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 416728, NULL);
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 503554, 'Elle');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 619934, 'Fay');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 735767, 'Gwen');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 882743, 'Heather');";

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
    }
}

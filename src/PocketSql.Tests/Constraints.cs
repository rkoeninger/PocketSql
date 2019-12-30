using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Constraints
    {
        [Test]
        public void IdentityColumn([All]IDbConnection connection)
        {
            connection.Execute(@"
                create table Things
                (
                    X int identity,
                    Y varchar(8)
                )");
            var id1 = connection.QuerySingle<int>("insert into Things (Y) values ('abc'); select @@identity");
            var id2 = connection.QuerySingle<int>("insert into Things (Y) values ('def'); select @@identity");
            var id3 = connection.QuerySingle<int>("insert into Things (Y) values ('ghi'); select @@identity");
            var things = connection.Query<Thing>("select * from Things");
            Assert.AreEqual(1, id1);
            Assert.AreEqual(2, id2);
            Assert.AreEqual(3, id3);
            Assert.IsTrue(new[]
            {
                new Thing(1, "abc"),
                new Thing(2, "def"),
                new Thing(3, "ghi")
            }.SequenceEqual(things));
        }

        [Test]
        public void IdentitySeedIncrementColumn([All]IDbConnection connection)
        {
            connection.Execute(@"
                create table Things
                (
                    X int identity(100, 10),
                    Y varchar(8)
                )");
            var id1 = connection.QuerySingle<int>("insert into Things (Y) values ('abc'); select @@identity");
            var id2 = connection.QuerySingle<int>("insert into Things (Y) values ('def'); select @@identity");
            var id3 = connection.QuerySingle<int>("insert into Things (Y) values ('ghi'); select @@identity");
            var things = connection.Query<Thing>("select * from Things");
            Assert.AreEqual(100, id1);
            Assert.AreEqual(110, id2);
            Assert.AreEqual(120, id3);
            Assert.IsTrue(new[]
            {
                new Thing(100, "abc"),
                new Thing(110, "def"),
                new Thing(120, "ghi")
            }.SequenceEqual(things));
        }

        [Test]
        public void DefaultColumn([All]IDbConnection connection)
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
    }
}

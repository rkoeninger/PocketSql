using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Inserts
    {
        [Test]
        public void InsertNull([All]IDbConnection connection)
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
        public void CreateAndInsertAndSelectCustomSchema([All]IDbConnection connection)
        {
            connection.Execute("create table abc.Things (X int, Y varchar(8))");
            connection.Execute("insert into abc.Things (X, Y) values (1, 'abc')");
            var thing = connection.QueryFirstOrDefault<Thing>("select * from abc.Things");
            Assert.AreEqual(new Thing(1, "abc"), thing);
        }
    }
}

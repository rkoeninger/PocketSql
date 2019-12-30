using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Deletes
    {
        [Test]
        public void InsertThenDelete([All]IDbConnection connection)
        {
            connection.Execute("create table People (FirstName varchar(16), LastName varchar(16))");
            connection.Execute("insert into People (FirstName, LastName) values ('Alison', 'Anderson')");
            connection.Execute("insert into People (FirstName, LastName) values ('Barbara', 'Billingsley')");
            connection.Execute("insert into People (FirstName, LastName) values ('Christine', 'Chapman')");
            connection.Execute("insert into People (FirstName, LastName) values ('Dennis', 'Anderson')");
            connection.Execute("insert into People (FirstName, LastName) values ('Earle', 'Billingsley')");
            connection.Execute("insert into People (FirstName, LastName) values ('Frank', 'Chapman')");
            Assert.AreEqual(6, connection.Query("select * from People").Count());
            Assert.AreEqual(2, connection.Execute("delete from People where LastName = 'Anderson'"));
            Assert.AreEqual(4, connection.Query("select * from People").Count());
            Assert.AreEqual(2, connection.Execute("delete from People where LastName = 'Billingsley'"));
            Assert.AreEqual(2, connection.Query("select * from People").Count());
            Assert.AreEqual(2, connection.Execute("delete from People where LastName = 'Chapman'"));
            Assert.AreEqual(0, connection.Query("select * from People").Count());
        }
    }
}

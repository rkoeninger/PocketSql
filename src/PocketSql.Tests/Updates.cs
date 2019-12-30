using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Updates
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
    }
}

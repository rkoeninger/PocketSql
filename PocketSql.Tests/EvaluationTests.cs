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
                connection.Execute("create table People ( Name varchar(32), Age int )");

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

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}

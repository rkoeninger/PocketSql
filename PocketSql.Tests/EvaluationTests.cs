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
            var engine = new Engine();

            using (var connection = engine.GetConnection())
            {
                Assert.AreEqual(-1, connection.Execute("create table People ( Name varchar(32), Age int )"));

                Assert.AreEqual(1, connection.Execute("insert into People (Name, Age) values ('Rob', 30)"));

                var people = connection.Query("select Name from People").ToList();
                Assert.AreEqual(1, people.Count);
                Assert.AreEqual("Rob", people[0].Name);
            }
        }
    }
}

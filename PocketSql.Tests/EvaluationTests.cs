using System;
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

                foreach (var row in connection.Query("select X, Y from Things order by X, Y desc"))
                {
                    Console.WriteLine($"{row.X}, {row.Y}");
                }
            }
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

                foreach (var x in connection.Query<int?>(@"
                    select X
                    from Numbers
                    order by X
                    offset 4 rows
                    fetch next 4 rows only"))
                {
                    Console.WriteLine(x);
                }
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

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}

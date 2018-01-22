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
                var createCommand = connection.CreateCommand();
                createCommand.CommandText = @"
                    create table People
                    (
                        Name varchar(32),
                        Age int
                    )
                ";
                Assert.AreEqual(-1, createCommand.ExecuteNonQuery());

                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"
                    insert into People
                    (
                        Name,
                        Age
                    )
                    values
                    (
                        'Rob',
                        30
                    )
                ";
                Assert.AreEqual(1, insertCommand.ExecuteNonQuery());

                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = @"
                    select * from People
                ";
                var reader = selectCommand.ExecuteReader();
                var totalRowCount = 0;

                while (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        totalRowCount++;
                        Assert.AreEqual("Rob", reader.GetString(reader.GetOrdinal("Name")));
                        Assert.AreEqual(30, reader.GetInt32(reader.GetOrdinal("Age")));
                    }
                }

                reader.Close();
                Assert.AreEqual(1, totalRowCount);
            }
        }
    }
}

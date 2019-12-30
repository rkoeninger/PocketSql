using System.Data;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Merges
    {
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
    }
}

using System.Data;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Variables
    {
        [Test]
        public void DeclareSetSelect([All]IDbConnection connection)
        {
            Assert.AreEqual("Rob", connection.ExecuteScalar<string>(@"
                declare @Name varchar(16)
                set @Name = 'Rob'
                select @Name"));
        }

        [Test]
        public void OutputParameter([All]IDbConnection connection)
        {
            var p = new DynamicParameters();
            p.Add("@a", 11);
            p.Add("@b", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@c", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
            connection.Execute(@"
                create proc TestProc
                    @a int,
                    @b int output
                as
                begin
                    set @b = 999
                    select 1111
                    return @a
                end");
            Assert.AreEqual(1111, connection.QueryFirst<int>(
                "TestProc",
                p,
                commandType: CommandType.StoredProcedure));
            Assert.AreEqual(11, p.Get<int>("@c"));
            Assert.AreEqual(999, p.Get<int>("@b"));
        }
    }
}

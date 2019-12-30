using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class UserDefined
    {
        [Test]
        public void StoredProc([AsOf(10)]IDbConnection connection)
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
            connection.Execute(@"
                create procedure DoIt
                    @Limit int
                as
                    select * from Things where X > @Limit");
            Assert.AreEqual(
                9,
                connection.Query(
                    "DoIt",
                    new { Limit = 50 },
                    commandType: CommandType.StoredProcedure).Count());
            connection.Execute(@"
                alter procedure DoIt
                    @Limit int
                as
                    select * from Things where X < @Limit");
            Assert.AreEqual(
                5,
                connection.Query(
                    "DoIt",
                    new { Limit = 40 },
                    commandType: CommandType.StoredProcedure).Count());
        }

        [Test]
        public void UserDefinedFunction([AsOf(9)]IDbConnection connection)
        {
            connection.Execute(@"
                create function ScalarMax(@X int, @Y int)
                returns int
                as
                begin
                    return (case when @X > @Y then @X else @Y end);
                end;");
            Assert.AreEqual(5, connection.QueryFirst<int>("select ScalarMax(3, 5)"));
        }
    }
}

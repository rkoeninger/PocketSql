using System.Data;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Globals
    {
        [Test]
        public void SelectRowCountGlobal([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(8, connection.ExecuteScalar<int>(@"
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
                (47, 'zxc');

                select @@rowcount;"));
        }
    }
}

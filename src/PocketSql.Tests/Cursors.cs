using System.Data;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Cursors
    {
        [Test]
        public void CursorFetchNext([AsOf(10)]IDbConnection connection)
        {
            connection.Execute("create table Things (X int, Y varchar(8))");
            Assert.AreEqual(8, connection.Execute(@"
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
                (47, 'zxc')"));
            var result = connection.ExecuteScalar<int>(@"
                declare @total int = 0
                declare cur cursor for select * from Things
                open cur

                fetch from cur into @X, @Y

                while @@fetch_status = 0
                begin
                    set @total += @X
                    fetch from cur into @X, @Y
                end

                close cur
                deallocate cur
                select @total");
            Assert.AreEqual(339, result);
        }
    }
}

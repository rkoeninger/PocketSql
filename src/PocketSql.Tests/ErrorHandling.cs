using System.Data;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class ErrorHandling
    {
        [Test]
        public void CatchAndSelectThrownError([AsOf(11)]IDbConnection connection)
        {
            var message = connection.ExecuteScalar<string>(@"
                begin try
                    throw 9999, 'whoops', 1;
                end try
                begin catch
                    select error_message();
                end catch");
            Assert.AreEqual("whoops", message);
        }

        [Test]
        public void ErrorInformationShouldBeNullOutsideOfCatch([All]IDbConnection connection)
        {
            Assert.IsNull(connection.ExecuteScalar<string>("select error_message()"));
            Assert.IsNull(connection.ExecuteScalar<int?>("select error_number()"));
            Assert.IsNull(connection.ExecuteScalar<int?>("select error_state()"));
            Assert.IsNull(connection.ExecuteScalar<int?>("select error_severity()"));
            Assert.IsNull(connection.ExecuteScalar<int?>("select error_line()"));
            Assert.IsNull(connection.ExecuteScalar<string>("select error_procedure()"));
        }
    }
}

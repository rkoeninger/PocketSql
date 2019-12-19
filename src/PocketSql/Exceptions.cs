using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace PocketSql
{
    public static class Exceptions
    {
        public static SqlException NewSqlException(int number, int state, string message, int version)
        {
            var collection = Construct<SqlErrorCollection>();
            var error = Construct<SqlError>(number, (byte)state, (byte)3, "PocketSql.Engine", message, "proc", 1, null);

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(collection, new object[] { error });

            return typeof(SqlException)
                .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    CallingConventions.ExplicitThis,
                    new[] { typeof(SqlErrorCollection), typeof(string) },
                    new ParameterModifier[] { })
                ?.Invoke(null, new object[] { collection, $"{version}.0.0" }) as SqlException;
        }

        private static T Construct<T>(params object[] p)
        {
            var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
        }
    }
}

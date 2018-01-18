using NUnit.Framework;

namespace PocketSql.Tests
{
    public class ParsingTests
    {
        [Test]
        public void Test()
        {
            ParseIt.LogTokens(@"
                select *
                from People
                where Age > 30");
        }

        [Test]
        public void Test2()
        {
            ParseIt.LogStatements(@"
                insert into People
                (Name, Age)
                values
                ('Rob', 30)

                select *
                from People
                where Age > 30");
        }
    }
}

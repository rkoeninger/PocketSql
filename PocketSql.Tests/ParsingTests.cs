using NUnit.Framework;

namespace PocketSql.Tests
{
    public class ParsingTests
    {
        [Test]
        public void Test()
        {
            ParseIt.Main(@"
                select *
                from People
                where Age > 30");
        }
    }
}

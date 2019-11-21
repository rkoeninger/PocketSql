using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    [TestFixture]
    public class SqlFunctionsForNullValueTests
    {
        [Test]
        public void TestIsNullFunction([AsOf(9)]IDbConnection connection)
        {
            connection.Execute(SampleTable);
            var ids = connection.Query<IdAndName>(@"
                SELECT Id, IsNull(Name, 'NO NAME GIVEN') AS 'Name'
                FROM [Sample]
                ORDER BY Name");
            Assert.IsNotNull(ids);
            Assert.AreEqual(8, ids.Count());
            Assert.AreEqual(1, ids.Where(x => x.Name == "NO NAME GIVEN").Count());
        }

        [Test]
        public void TestNullIfFunction([AsOf(9)]IDbConnection connection)
        {
            connection.Execute(SampleTable);
            var ids = connection.Query<IdAndName>(@"
                SELECT Id, NullIf(Name, 'Gwen') AS 'Name'
                FROM [Sample]
                ORDER BY Name");
            Assert.IsNotNull(ids);
            Assert.AreEqual(8, ids.Count());
            Assert.AreEqual(2, ids.Where(x => string.IsNullOrEmpty(x.Name)).Count());
            Assert.IsFalse(ids.Any(x => x.Name == "Gwen"));
        }

        internal class IdAndName
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        private const string SampleTable = @"
            CREATE TABLE [Sample]
            (
                [ID] [int] IDENTITY(1000, 1) NOT FOR REPLICATION NOT NULL,
                [Name] [varchar](80) NOT NULL
            );
      
            INSERT INTO [Sample] ([ID], [Name]) VALUES ( 116725, 'Alison');
            INSERT INTO [Sample] ([ID], [Name]) VALUES ( 216726, 'Barbara');
            INSERT INTO [Sample] ([ID], [Name]) VALUES ( 316727, 'Chrisine');
            INSERT INTO [Sample] ([ID], [Name]) VALUES ( 416728, NULL);
            INSERT INTO [Sample] ([ID], [Name]) VALUES ( 503554, 'Elle');
            INSERT INTO [Sample] ([ID], [Name]) VALUES ( 619934, 'Fay');
            INSERT INTO [Sample] ([ID], [Name]) VALUES ( 735767, 'Gwen');
            INSERT INTO [Sample] ([ID], [Name]) VALUES ( 882743, 'Heather');";
    }
}

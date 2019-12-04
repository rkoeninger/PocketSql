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
                ORDER BY Name")?.ToList();
            Assert.IsNotNull(ids);
            Assert.AreEqual(8, ids.Count);
            Assert.AreEqual(1, ids.Count(x => x.Name == "NO NAME GIVEN"));
        }

        [Test]
        public void TestNullIfFunction([AsOf(9)]IDbConnection connection)
        {
            connection.Execute(SampleTable);
            var ids = connection.Query<IdAndName>(@"
                SELECT Id, NullIf(Name, 'Gwen') AS 'Name'
                FROM [Sample]
                ORDER BY Name")?.ToList();
            Assert.IsNotNull(ids);
            Assert.AreEqual(8, ids.Count);
            Assert.AreEqual(2, ids.Count(x => string.IsNullOrEmpty(x.Name)));
            Assert.IsFalse(ids.Any(x => x.Name == "Gwen"));
        }

        internal class IdAndName
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private const string SampleTable = @"
            CREATE TABLE [Sample]
            (
                [Id] [int] IDENTITY(1000, 1) NOT FOR REPLICATION NOT NULL,
                [Name] [varchar](80) NOT NULL
            );
      
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 116725, 'Alison');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 216726, 'Barbara');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 316727, 'Chrisine');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 416728, NULL);
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 503554, 'Elle');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 619934, 'Fay');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 735767, 'Gwen');
            INSERT INTO [Sample] ([Id], [Name]) VALUES ( 882743, 'Heather');";
    }
}

using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Comparisons
    {
        [Test]
        public void TestWhereStringGreaterThanString([All]IDbConnection connection)
        {
            connection.Execute(SampleTable);
            var ids = connection.Query<IdAndName>("SELECT * FROM [Sample] WHERE [Name] > 'E'")?.ToList();
            Assert.IsNotNull(ids);
            Assert.AreEqual(4, ids?.Count);
            Assert.IsFalse(ids?.Any(x => x.Name.CompareTo("E") <= 0));
        }

        [Test]
        public void TestWhereStringLessThanString([All]IDbConnection connection)
        {
            connection.Execute(SampleTable);
            var ids = connection.Query<IdAndName>("SELECT * FROM [Sample] WHERE [Name] < 'E'")?.ToList();
            Assert.IsNotNull(ids);
            Assert.AreEqual(4, ids?.Count);
            Assert.IsFalse(ids?.Any(x => x.Name.CompareTo("E") >= 0));
        }

        private const string SampleTable = @"
            CREATE TABLE [Sample]
            (
                [Id] [int] IDENTITY(1000, 1) NOT FOR REPLICATION NOT NULL,
                [Name] [varchar](80) NOT NULL
            );

            INSERT INTO [Sample] ([Id], [Name]) VALUES (116725, 'Alison');
            INSERT INTO [Sample] ([Id], [Name]) VALUES (216726, 'Barbara');
            INSERT INTO [Sample] ([Id], [Name]) VALUES (316727, 'Chrisine');
            INSERT INTO [Sample] ([Id], [Name]) VALUES (416728, 'Debra');
            INSERT INTO [Sample] ([Id], [Name]) VALUES (503554, 'Elle');
            INSERT INTO [Sample] ([Id], [Name]) VALUES (619934, 'Fay');
            INSERT INTO [Sample] ([Id], [Name]) VALUES (735767, 'Gwen');
            INSERT INTO [Sample] ([Id], [Name]) VALUES (882743, 'Heather');";
    }
}

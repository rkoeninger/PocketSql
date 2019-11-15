using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    [TestFixture]
    public class WhereConditionTests
    {
        [Test]
        public void TestWhereStringGreaterThanString()
        {
            var engine = new Engine(140);
            using (IDbConnection conn = engine.GetConnection())
            {
                conn.Execute(SampleTable);
                var ids = conn.Query<IdAndName>("SELECT * FROM [Sample] WHERE [Name] > 'E'");
                Assert.IsNotNull(ids);
                Assert.AreEqual(4, ids.Count());
                Assert.IsFalse(ids.Any(x => x.Name.CompareTo("E")<=0));
            }
        }
      
        [Test]
        public void TestWhereStringLessThanString()
        {
            var engine = new Engine(140);
            using (IDbConnection conn = engine.GetConnection())
            {
                conn.Execute(SampleTable);
                var ids = conn.Query<IdAndName>("SELECT * FROM [Sample] WHERE [Name] < 'E'");
                Assert.IsNotNull(ids);
                Assert.AreEqual(4, ids.Count());
                Assert.IsFalse(ids.Any(x => x.Name.CompareTo("E") >= 0));
            }
        }
      
        internal class IdAndName
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        private const string SampleTable = @"
          CREATE TABLE [Sample] (
              [ID] [int] IDENTITY(1000,1) NOT FOR REPLICATION NOT NULL,
              [Name] [varchar](80) NOT NULL,
              );
      
          INSERT INTO [Sample] ([ID],[Name]) VALUES ( 116725, 'Alison')
          INSERT INTO [Sample] ([ID],[Name]) VALUES ( 216726, 'Barbara')
          INSERT INTO [Sample] ([ID],[Name]) VALUES ( 316727, 'Chrisine')
          INSERT INTO [Sample] ([ID],[Name]) VALUES ( 416728, 'Debra')
          INSERT INTO [Sample] ([ID],[Name]) VALUES ( 503554, 'Elle')
          INSERT INTO [Sample] ([ID],[Name]) VALUES ( 619934, 'Fay')
          INSERT INTO [Sample] ([ID],[Name]) VALUES ( 735767, 'Gwen')
          INSERT INTO [Sample] ([ID],[Name]) VALUES ( 882743, 'Heather')
        ";
    }
}

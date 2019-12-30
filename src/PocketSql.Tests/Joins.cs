using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace PocketSql.Tests
{
    public class Joins
    {
        private static void CreateEngineWithCityColorTaste(IDbConnection connection)
        {
            connection.Execute(@"
                create table TableA (X int, City varchar(32))

                insert into TableA
                (X, City)
                values
                (1, 'atlanta'),
                (2, 'boston'),
                (3, 'chicago'),
                (4, 'dallas'),
                (5, 'erie'),
                (6, 'frankfort'),
                (7, 'geneva'),
                (8, 'hamburg'),
                (9, 'indianapolis'),
                (10, 'jakarta'),
                (11, 'knoxville'),
                (12, 'leipzig')

                create table TableB (X int, Y int, Color varchar(12))

                insert into TableB
                (X, Y, Color)
                values
                (1, 26, 'red'),
                (2, 25, 'blue'),
                (4, 25, 'green'),
                (5, 24, 'yellow'),
                (7, 22, 'purple'),
                (8, 22, 'orange'),
                (10, 21, 'purple'),
                (12, 21, 'gray')

                create table TableC (Y int, Taste varchar(8))

                insert into TableC
                (Y, Taste)
                values
                (21, 'sweet'),
                (22, 'spicy'),
                (23, 'sour'),
                (24, 'bitter'),
                (25, 'savory'),
                (26, 'salty')");
        }

        [Test]
        public void InnerJoin([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select a.City, b.Color, c.Taste
                from TableA as a
                join TableB as b on a.X = b.X
                join TableC as c on b.Y = c.Y").ToList();
            Assert.AreEqual(8, results.Count);
        }

        [Test]
        public void OuterJoin([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select b.Color, c.Taste
                from TableB as b
                full outer join TableC as c on b.Y = c.Y").ToList();
            Assert.AreEqual(9, results.Count);
        }

        [Test]
        public void CrossJoin([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select *
                from TableA as a
                cross join TableB as b").ToList();
            Assert.AreEqual(96, results.Count);
        }

        [Test]
        public void CrossApply([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select * from TableA as a
                cross apply
                (select * from TableB as b) as c").ToList();
            Assert.AreEqual(96, results.Count);
        }

        [Test]
        public void OuterApply([AsOf(10)]IDbConnection connection)
        {
            CreateEngineWithCityColorTaste(connection);
            var results = connection.Query<CityColorTaste>(@"
                select * from TableA as a
                outer apply
                (select * from TableB as b) as c").ToList();
            Assert.AreEqual(96, results.Count);
        }
    }
}

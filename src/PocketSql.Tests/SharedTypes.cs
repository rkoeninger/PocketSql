using System;

namespace PocketSql.Tests
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class IdAndName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CityColorTaste
    {
        public string City { get; set; }
        public string Color { get; set; }
        public string Taste { get; set; }
    }

    public class Thing : IEquatable<Thing>
    {
        public Thing() { }

        public Thing(int x, string y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public string Y { get; set; }

        public override bool Equals(object that) => that is Thing thing && Equals(thing);

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ (Y != null ? Y.GetHashCode() : 0);
            }
        }

        public bool Equals(Thing that) => that != null && X == that.X && Y == that.Y;
    }
}

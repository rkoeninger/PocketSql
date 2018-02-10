namespace PocketSql
{
    public static class Equality
    {
        public static bool Equal(object x, object y) => x != null && y != null && Equals(x, y);
    }
}

namespace PocketSql
{
    public static class Naming
    {
        public static string Parameter(string name) => name.StartsWith("@") ? name : $"@{name}";
    }
}

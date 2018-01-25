using System;

namespace PocketSql
{
    public static class CaseInsensitivity
    {
        public static bool Similar(this string x, string y) =>
            string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    }
}

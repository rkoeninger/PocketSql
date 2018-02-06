using System;
using System.Collections.Generic;

namespace PocketSql
{
    public static class CaseInsensitivity
    {
        public static bool Similar(this string x, string y) =>
            string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

        public class EqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y) => Similar(x, y);
            public int GetHashCode(string obj) => obj.ToLower().GetHashCode();
        }
    }
}

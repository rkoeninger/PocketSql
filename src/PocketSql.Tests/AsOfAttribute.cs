using NUnit.Framework;

namespace PocketSql.Tests
{
    public class AsOfAttribute : RangeAttribute
    {
        public AsOfAttribute(int version) : base(version, 14)
        {
        }
    }
}

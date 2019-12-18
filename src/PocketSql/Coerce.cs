using System;

namespace PocketSql
{
    public static class Coerce
    {
        // TODO: move conversion logic here
        public static T To<T>(this object x)
        {
            return default;
        }

        // TODO: define system functions in dictionary dispatched by name and closest type
        //       will have to determine which overload has the closest types and then coerce
        //       one or both arguments to match parameter types
        // (>) : (int, int) => int
        //       (long, long) => long
        //       (double, double) => double
    }
}

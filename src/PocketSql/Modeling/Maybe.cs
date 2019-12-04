using System;

namespace PocketSql.Modeling
{
    public static class Maybe
    {
        public static Maybe<A> None<A>() => Maybe<A>.None;
        public static Maybe<A> Some<A>(A x) => new Maybe<A>(true, x);
    }

    public struct Maybe<A>
    {
        public static readonly Maybe<A> None = new Maybe<A>();

        private bool HasValue { get; }
        private A Value { get; }

        public Maybe(bool hasValue, A value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public Maybe<B> Select<B>(Func<A, B> f) => new Maybe<B>(HasValue, HasValue ? f(Value) : default);
        public Maybe<A> Or(Func<Maybe<A>> f) => HasValue ? this : f();
        public A OrElse(Func<A> f) => HasValue ? Value : f();
    }
}

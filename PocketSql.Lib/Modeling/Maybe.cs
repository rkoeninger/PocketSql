using System;

namespace PocketSql.Modeling
{
    public static class Maybe
    {
        public static Maybe<A> None<A>() => Maybe<A>.None;
        public static Maybe<A> Some<A>(A x) => new Maybe<A>(true, x);
        public static Maybe<A> FromNull<A>(A x) => x == null ? Maybe<A>.None : new Maybe<A>(true, x);
    }

    public struct Maybe<A>
    {
        public static readonly Maybe<A> None = new Maybe<A>();

        public bool HasValue { get; }
        private A Value { get; }

        public Maybe(bool hasValue, A value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public Maybe<B> Select<B>(Func<A, B> f) => new Maybe<B>(HasValue, HasValue ? f(Value) : default);
        public Maybe<B> SelectMany<B>(Func<A, Maybe<B>> f) => HasValue ? f(Value) : default;
        public Maybe<A> Where(Func<A, bool> f) => HasValue && f(Value) ? this : None;
        public Maybe<A> Or(Maybe<A> m) => HasValue ? this : m;
        public Maybe<A> Or(Func<Maybe<A>> f) => HasValue ? this : f();
        public A OrElse(A x) => HasValue ? Value : x;
        public A OrElse(Func<A> f) => HasValue ? Value : f();
        public Maybe<B> Cast<B>() => HasValue ? new Maybe<B>(true, (B)((object)Value)) : Maybe<B>.None;
        public Maybe<B> OfType<B>() => HasValue && Value is B ? Cast<B>() : Maybe<B>.None;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Immutable value type that captures 10^<see cref="Exp10"/>.2^<see cref="Exp2"/>.
    /// </summary>
    public struct ExpFactor : IComparable<ExpFactor>, IEquatable<ExpFactor>
    {
        /// <summary>
        /// The neutral factor (0,0).
        /// </summary>
        public static readonly ExpFactor Neutral;

        /// <summary>
        /// The base 2 exponent.
        /// </summary>
        public readonly int Exp2;

        /// <summary>
        /// The base 10 exponent.
        /// </summary>
        public readonly int Exp10;

        /// <summary>
        /// Gets whether this is the neutral factor.
        /// </summary>
        public bool IsNeutral => Exp2 == 0 && Exp10 == 0;

        /// <summary>
        /// Initializes a new <see cref="ExpFactor"/>.
        /// </summary>
        /// <param name="exp2">The base 2 exponent.</param>
        /// <param name="exp10">The base 10 exponent.</param>
        public ExpFactor( int exp2, int exp10 )
        {
            Exp2 = exp2;
            Exp10 = exp10;
        }

        public ExpFactor Power( int p ) => new ExpFactor( Exp2 * p, Exp10 * p );

        public ExpFactor Multiply( ExpFactor x ) => new ExpFactor( Exp2 + x.Exp2, Exp10 + x.Exp10 );

        public ExpFactor DivideBy( ExpFactor x ) => new ExpFactor( Exp2 - x.Exp2, Exp10 - x.Exp10 );

        public double ToDouble() => Math.Pow( 2, Exp2 ) * Math.Pow( 10, Exp10 );

        /// <summary>
        /// Gets the string representation. Can be the empty sring (if <see cref="IsNeutral"/>) or
        /// the "10^<see cref="Exp10"/>.2^<see cref="Exp2"/>" optionally prefixed by a '*'.
        /// </summary>
        /// <param name="withMultPrefix">True to prefix by '*' when <see cref="IsNeutral"/> is false.</param>
        /// <returns>A readable string.</returns>
        public string ToString( bool withMultPrefix )
        {
            string s = String.Empty;
            if( Exp10 != 0 )
            {
                s = (withMultPrefix ? "*10^" : "10^") + Exp10;
                if( Exp2 != 0 ) s += "*2^" + Exp2;
            }
            else if( Exp2 != 0 ) s = (withMultPrefix ? ".2^" : "2^") + Exp2;
            return s;
        }

        /// <summary>
        /// See <see cref="ToString(bool)"/> that is called with false (no multpilcation prefix).
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => ToString( false );

        public override bool Equals( object obj ) => obj is ExpFactor f && Equals( f );

        public bool Equals( ExpFactor other ) => Exp2 == other.Exp2 && Exp10 == other.Exp10;

        public override int GetHashCode() => Exp10 << 7 ^ Exp2;

        public int CompareTo( ExpFactor other )
        {
            int cmp = Exp10 - other.Exp10;
            return cmp == 0 ? Exp2 - other.Exp2 : cmp;
        }

    }
}

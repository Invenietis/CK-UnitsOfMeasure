using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CK.UnitsOfMeasure
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
        public readonly short Exp2;

        /// <summary>
        /// The base 10 exponent.
        /// </summary>
        public readonly short Exp10;

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
            checked
            {
                Exp2 = (short)exp2;
                Exp10 = (short)exp10;
            }
        }

        /// <summary>
        /// Elevates this factor to a given power.
        /// </summary>
        /// <param name="p">The power.</param>
        /// <returns>This factor elevated to <paramref name="p"/>.</returns>
        public ExpFactor Power( int p ) => new ExpFactor( Exp2 * p, Exp10 * p );

        /// <summary>
        /// Multplies this factor by another one.
        /// </summary>
        /// <param name="x">The factor.</param>
        /// <returns>This factor multipied by <paramref name="x"/>.</returns>
        public ExpFactor Multiply( ExpFactor x ) => new ExpFactor( Exp2 + x.Exp2, Exp10 + x.Exp10 );

        /// <summary>
        /// Divides this factor by another one.
        /// </summary>
        /// <param name="x">The divisor.</param>
        /// <returns>This factor divided by <paramref name="x"/>.</returns>
        public ExpFactor DivideBy( ExpFactor x ) => new ExpFactor( Exp2 - x.Exp2, Exp10 - x.Exp10 );

        /// <summary>
        /// Computes a double value.
        /// </summary>
        /// <returns>The double value.</returns>
        public double ToDouble() => Math.Pow( 2, Exp2 ) * Math.Pow( 10, Exp10 );

        /// <summary>
        /// Gets the string representation. Can be the empty string (if <see cref="IsNeutral"/>) or
        /// the "10^<see cref="Exp10"/>.2^<see cref="Exp2"/>" optionally prefixed by <paramref name="mult"/>.
        /// </summary>
        /// <param name="mult">Multipication character ('\0' for none, typically '*' or '.').</param>
        /// <returns>A readable string.</returns>
        public string ToString( char mult )
        {
            string s = String.Empty;
            if( Exp10 != 0 )
            {
                s = (mult != '\0' ? mult + "10^" : "10^") + Exp10.ToString( CultureInfo.InvariantCulture );
                if( Exp2 != 0 ) s += (mult == '\0' ? '.' : mult) + "2^" + Exp2.ToString( CultureInfo.InvariantCulture );
            }
            else if( Exp2 != 0 ) s = (mult != '\0' ? mult + "2^" : "2^") + Exp2.ToString( CultureInfo.InvariantCulture );
            return s;
        }

        /// <summary>
        /// See <see cref="ToString(char)"/> that is called with no multiplication prefix (the '\0' character).
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => ToString( '\0' );

        /// <summary>
        /// Implements value semantic equality.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if <paramref name="obj"/> is the same as this.</returns>
        public override bool Equals( object obj ) => obj is ExpFactor f && Equals( f );

        /// <summary>
        /// Implements value semantic equality.
        /// </summary>
        /// <param name="other">The other factor.</param>
        /// <returns>True if <paramref name="other"/> is the same as this.</returns>
        public bool Equals( ExpFactor other ) => Exp2 == other.Exp2 && Exp10 == other.Exp10;

        /// <summary>
        /// Implements value semantic equality.
        /// </summary>
        /// <returns>The hash code based on <see cref="Exp10"/> and <see cref="Exp2"/>.</returns>
        public override int GetHashCode() => Exp10 << 7 ^ Exp2;

        /// <summary>
        /// Compares this factor to another one, <see cref="Exp10"/> being the primary
        /// key.
        /// </summary>
        /// <param name="other">The other factor to compare to.</param>
        /// <returns>Positive if this is greater than other, 0 if they are equal and negative otherwise.</returns>
        public int CompareTo( ExpFactor other )
        {
            int cmp = Exp10 - other.Exp10;
            return cmp == 0 ? Exp2 - other.Exp2 : cmp;
        }

    }
}

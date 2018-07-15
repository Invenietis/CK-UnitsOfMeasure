using System;
using System.Collections.Generic;
using System.Text;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// Immutable struct that captures <see cref="Factor"/>.2^<see cref="Exp2"/>.10^<see cref="Exp10"/>.
    /// </summary>
    public readonly struct FullFactor : IEquatable<FullFactor>
    {
        /// <summary>
        /// The neutral factor: <see cref="Factor"/> = 1 and <see cref="ExpFactor.Neutral"/>.
        /// </summary>
        public static readonly FullFactor Neutral = new FullFactor( 1.0, ExpFactor.Neutral );

        /// <summary>
        /// The default factor is 0 (unfortunaletly).
        /// </summary>
        public static readonly FullFactor Zero;

        /// <summary>
        /// The linear factor.
        /// </summary>
        public readonly double Factor;

        /// <summary>
        /// The exponent factor.
        /// </summary>
        public readonly ExpFactor ExpFactor;

        /// <summary>
        /// Gets whether this is the neutral factor.
        /// </summary>
        public bool IsNeutral => Factor == 1.0 && ExpFactor.IsNeutral;

        /// <summary>
        /// Gets whether this <see cref="Factor"/> is 0.
        /// </summary>
        public bool IsZero => Factor == 0.0;

        /// <summary>
        /// Initializes a new <see cref="FullFactor"/>.
        /// </summary>
        /// <param name="f">The factor.</param>
        /// <param name="e">The exponent factor.</param>
        public FullFactor( double f, ExpFactor e )
        {
            Factor = f;
            ExpFactor = e;
        }

        /// <summary>
        /// Initializes a new <see cref="FullFactor"/> with a <see cref="ExpFactor.Neutral"/>.
        /// </summary>
        /// <param name="f">The factor.</param>
        public FullFactor( double f )
            : this( f, ExpFactor.Neutral )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="FullFactor"/> with a <see cref="Factor"/> = 1.0.
        /// </summary>
        /// <param name="f">The exponent factor.</param>
        public FullFactor( ExpFactor f )
            : this( 1.0, f )
        {
        }

        public static implicit operator FullFactor( double d ) => new FullFactor( d );
        public static implicit operator FullFactor( ExpFactor e ) => new FullFactor( e );

        /// <summary>
        /// Returns this factor elevated to a given power.
        /// </summary>
        /// <param name="p">The power.</param>
        /// <returns>The elevated full factor.</returns>
        public FullFactor Power( int p ) => new FullFactor( Math.Pow( Factor, p ), ExpFactor.Power( p ) );

        /// <summary>
        /// Returns this factor multiplied by another one.
        /// </summary>
        /// <param name="x">The factor to multiply with.</param>
        /// <returns>The resulting full factor.</returns>
        public FullFactor Multiply( FullFactor x ) => new FullFactor( Factor * x.Factor, ExpFactor.Multiply( x.ExpFactor ) );

        /// <summary>
        /// Returns this factor divide by another one.
        /// </summary>
        /// <param name="x">The divisor.</param>
        /// <returns>The resulting full factor.</returns>
        public FullFactor DivideBy( FullFactor x ) => new FullFactor( Factor / x.Factor, ExpFactor.DivideBy( x.ExpFactor ) );

        public double ToDouble() => Factor * ExpFactor.ToDouble();

        public override string ToString()
        {
            if( IsNeutral ) return String.Empty;
            if( IsZero ) return "0";
            if( Factor == 1.0 ) return ExpFactor.ToString();
            return Factor.ToString() + ExpFactor.ToString( true );
        }

        public static bool operator ==( FullFactor f1, FullFactor f2 ) => f1.Equals( f2 );

        public static bool operator !=( FullFactor f1, FullFactor f2 ) => !f1.Equals( f2 );

        public override bool Equals( object obj ) => obj is FullFactor f && Equals( f );

        public bool Equals( FullFactor other ) => Factor == other.Factor && ExpFactor.Equals( other.ExpFactor );

        public override int GetHashCode() => Factor.GetHashCode() ^ ExpFactor.GetHashCode();

    }
}

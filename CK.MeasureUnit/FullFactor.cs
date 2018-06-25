using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Immutable struc that captures 2^<see cref="Exp2"/>.10^<see cref="Exp10"/>.
    /// </summary>
    public struct FullFactor : IEquatable<FullFactor>
    {
        /// <summary>
        /// The neutral factor: <see cref="Factor"/> = 1 and <see cref="ExpFactor.Neutral"/>.
        /// </summary>
        public static readonly FullFactor Neutral;

        /// <summary>
        /// The (unfortunaletly) default factor is 0.
        /// </summary>
        public static readonly FullFactor Zero;

        /// <summary>
        /// The linear factor.
        /// </summary>
        public double Factor;

        /// <summary>
        /// The exponent factor.
        /// </summary>
        public ExpFactor ExpFactor;

        /// <summary>
        /// Gets whether this is the neutral factor.
        /// </summary>
        public bool IsNeutral => Factor == 1 && ExpFactor.IsNeutral;

        /// <summary>
        /// Gets whether this <see cref="Factor"/> is 0.
        /// </summary>
        public bool IsZero => Factor == 0.0;

        /// <summary>
        /// Initializes a new <see cref="FullFactor"/>.
        /// </summary>
        /// <param name="exp2">The base 2 exponent.</param>
        /// <param name="exp10">The base 10 exponent.</param>
        public FullFactor( double f, ExpFactor e )
        {
            Factor = f;
            ExpFactor = e;
        }

        public FullFactor Power( int p ) => new FullFactor( Math.Pow( Factor, p ), ExpFactor.Power( p ) );

        public FullFactor Multiply( FullFactor x ) => new FullFactor( Factor * x.Factor, ExpFactor.Multiply( x.ExpFactor ) );

        public FullFactor DivideBy( FullFactor x ) => new FullFactor( Factor / x.Factor, ExpFactor.DivideBy( x.ExpFactor ) );

        public double ToDouble() => Factor * ExpFactor.ToDouble();

        public override string ToString()
        {
            if( IsNeutral ) return String.Empty;
            if( IsZero ) return "0";
            if( Factor == 1.0 ) return ExpFactor.ToString();
            return Factor.ToString() + ExpFactor.ToString( true );
        }

        public override bool Equals( object obj ) => obj is FullFactor f && Equals( f );

        public bool Equals( FullFactor other ) => Factor == other.Factor && ExpFactor.Equals( other.ExpFactor );

        public override int GetHashCode() => Factor.GetHashCode() ^ ExpFactor.GetHashCode();

    }
}

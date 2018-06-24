using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Normalized measure unit describes any normalized combination of one or more <see cref="BasicMeasureUnit"/>.
    /// </summary>
    public class NormalizedMeasureUnit : MeasureUnit
    {
        readonly BasicMeasureUnit[] _units;
        NormalizedMeasureUnit _invert;

        internal NormalizedMeasureUnit( (string A, string N) names, BasicMeasureUnit[] units )
            : base( names.A, names.N )
        {
            _units = units;
        }

        private protected NormalizedMeasureUnit( string abbreviation, string name )
            : base( abbreviation, name )
        {
            _units = new[] { (BasicMeasureUnit)this };
        }

        /// <summary>
        /// Gets the <see cref="BasicMeasureUnit"/> that define this normalized measure.
        /// </summary>
        public IReadOnlyList<BasicMeasureUnit> MeasureUnits => _units;

        /// <summary>
        /// Returns the normalized units that results from this one multiplied by another normalized units.
        /// </summary>
        /// <param name="m">Other units to multiply.</param>
        /// <returns>The result of the multiplication.</returns>
        public NormalizedMeasureUnit Multiply( NormalizedMeasureUnit m ) => SafeCreate( MeasureUnits.Concat( m.MeasureUnits ) );

        /// <summary>
        /// Returns the normalized units that results from this one divided by another normalized units.
        /// </summary>
        /// <param name="m">The divisor.</param>
        /// <returns>The result of the division.</returns>
        public NormalizedMeasureUnit DivideBy( NormalizedMeasureUnit m ) => SafeCreate( MeasureUnits.Concat( m.Invert().MeasureUnits ) );

        public static NormalizedMeasureUnit operator /( NormalizedMeasureUnit o1, NormalizedMeasureUnit o2 ) => o1.DivideBy( o2 );
        public static NormalizedMeasureUnit operator *( NormalizedMeasureUnit o1, NormalizedMeasureUnit o2 ) => o1.Multiply( o2 );
        public static NormalizedMeasureUnit operator ^( NormalizedMeasureUnit o, int exp ) => o.Power( exp );

        /// <summary>
        /// Inverts this normalized units.
        /// </summary>
        /// <returns>The inverted units.</returns>
        public NormalizedMeasureUnit Invert()
        {
            if( _invert == null )
            {
                Combinator c = new Combinator( Array.Empty<BasicMeasureUnit>() );
                foreach( var m in MeasureUnits )
                {
                    if( m.MeasureUnit != None ) c.Add( m.MeasureUnit, -m.Exponent );
                }
                if( _invert == null ) _invert = c.GetResult();
            }
            return _invert;
        }

        /// <summary>
        /// Returns this normalized units elevated to a given power.
        /// Note that when <paramref name="exp"/> is 0, <see cref="MeasureUnit.None"/> is returned.
        /// </summary>
        /// <param name="exp">The exponent.</param>
        /// <returns>The resulting normalized units.</returns>
        public NormalizedMeasureUnit Power( int exp )
        {
            if( exp == 0 ) return None;
            if( exp == 1 ) return this;
            if( exp == -1 ) return Invert();
            Combinator c = new Combinator( Array.Empty<BasicMeasureUnit>() );
            foreach( var m in MeasureUnits )
            {
                if( m.MeasureUnit != None ) c.Add( m.MeasureUnit, m.Exponent * exp );
            }
            return c.GetResult();
        }

        internal static (string A, string N) ComputeNames( BasicMeasureUnit[] units )
        {
            Debug.Assert( units.Length > 0 && units.All( m => m != null ) && units.Length > 1 );
            return (String.Join( ".", units.Select( u => u.Abbreviation ) ), String.Join( ".", units.Select( u => u.Name ) ));
        }

        struct Combinator
        {
            readonly List<FundamentalMeasureUnit> _normM;
            readonly List<int> _normE;

            public Combinator( IEnumerable<BasicMeasureUnit> units )
            {
                _normM = new List<FundamentalMeasureUnit>();
                _normE = new List<int>();
                foreach( var u in units )
                {
                    Debug.Assert( u != null );
                    if( u.MeasureUnit != FundamentalMeasureUnit.None ) Add( u.MeasureUnit, u.Exponent );
                }
            }

            public void Add( FundamentalMeasureUnit u, int exp )
            {
                for( int i = 0; i < _normM.Count; ++i )
                {
                    if( _normM[i] == u )
                    {
                        _normE[i] += exp;
                        return;
                    }
                }
                _normM.Add( u );
                _normE.Add( exp );
            }

            public NormalizedMeasureUnit GetResult()
            {
                int count = _normM.Count;
                if( count == 0 ) return None;
                if( count == 1 )
                {
                    int exp = _normE[0];
                    return exp != 0 ? RegisterBasic( exp, _normM[0] ) : None;
                }
                var result = new List<BasicMeasureUnit>( count );
                for( int i = 0; i < count; ++i )
                {
                    int exp = _normE[i];
                    if( exp != 0 ) result.Add( RegisterBasic( exp, _normM[i] ) );
                }
                count = result.Count;
                if( count == 0 ) return None;
                if( count == 1 ) return result[0];
                result.Sort();
                return RegisterNormalized( result.ToArray() );
            }
        }

        internal static NormalizedMeasureUnit SafeCreate( IEnumerable<BasicMeasureUnit> units )
        {
            return new Combinator( units ).GetResult();
        }
    }
}

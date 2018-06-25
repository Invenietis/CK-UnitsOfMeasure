using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Combined measure unit: any combination of one or more <see cref="ExponentMeasureUnit"/>.
    /// </summary>
    public class CombinedMeasureUnit : MeasureUnit
    {
        readonly ExponentMeasureUnit[] _units;
        CombinedMeasureUnit _invert;

        internal CombinedMeasureUnit( (string A, string N) names, ExponentMeasureUnit[] units )
            : base( names.A, names.N )
        {
            _units = units;
        }

        private protected CombinedMeasureUnit( string abbreviation, string name )
            : base( abbreviation, name )
        {
            _units = new[] { (ExponentMeasureUnit)this };
        }

        /// <summary>
        /// Gets the <see cref="ExponentMeasureUnit"/> that define this normalized measure.
        /// </summary>
        public IReadOnlyList<ExponentMeasureUnit> MeasureUnits => _units;

        /// <summary>
        /// Returns the normalized units that results from this one multiplied by another normalized units.
        /// </summary>
        /// <param name="m">Other units to multiply.</param>
        /// <returns>The result of the multiplication.</returns>
        public CombinedMeasureUnit Multiply( CombinedMeasureUnit m ) => SafeCreate( MeasureUnits.Concat( m.MeasureUnits ) );

        /// <summary>
        /// Returns the normalized units that results from this one divided by another normalized units.
        /// </summary>
        /// <param name="m">The divisor.</param>
        /// <returns>The result of the division.</returns>
        public CombinedMeasureUnit DivideBy( CombinedMeasureUnit m ) => SafeCreate( MeasureUnits.Concat( m.Invert().MeasureUnits ) );

        public static CombinedMeasureUnit operator /( CombinedMeasureUnit o1, CombinedMeasureUnit o2 ) => o1.DivideBy( o2 );
        public static CombinedMeasureUnit operator *( CombinedMeasureUnit o1, CombinedMeasureUnit o2 ) => o1.Multiply( o2 );
        public static CombinedMeasureUnit operator ^( CombinedMeasureUnit o, int exp ) => o.Power( exp );

        /// <summary>
        /// Inverts this normalized units.
        /// </summary>
        /// <returns>The inverted units.</returns>
        public CombinedMeasureUnit Invert()
        {
            if( _invert == null )
            {
                Combinator c = new Combinator( Array.Empty<ExponentMeasureUnit>() );
                foreach( var m in MeasureUnits )
                {
                    if( m.AtomicMeasureUnit != None ) c.Add( m.AtomicMeasureUnit, -m.Exponent );
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
        public CombinedMeasureUnit Power( int exp )
        {
            if( exp == 0 ) return None;
            if( exp == 1 ) return this;
            if( exp == -1 ) return Invert();
            Combinator c = new Combinator( Array.Empty<ExponentMeasureUnit>() );
            foreach( var m in MeasureUnits )
            {
                if( m.AtomicMeasureUnit != None ) c.Add( m.AtomicMeasureUnit, m.Exponent * exp );
            }
            return c.GetResult();
        }

        internal static (string A, string N) ComputeNames( ExponentMeasureUnit[] units )
        {
            Debug.Assert( units.Length > 0 && units.All( m => m != null ) && units.Length > 1 );
            return (String.Join( ".", units.Select( u => u.Abbreviation ) ), String.Join( ".", units.Select( u => u.Name ) ));
        }

        struct Combinator
        {
            readonly List<AtomicMeasureUnit> _normM;
            readonly List<int> _normE;

            public Combinator( IEnumerable<ExponentMeasureUnit> units )
            {
                _normM = new List<AtomicMeasureUnit>();
                _normE = new List<int>();
                foreach( var u in units )
                {
                    Debug.Assert( u != null );
                    if( u.AtomicMeasureUnit != FundamentalMeasureUnit.None ) Add( u.AtomicMeasureUnit, u.Exponent );
                }
            }

            public void Add( AtomicMeasureUnit u, int exp )
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

            public CombinedMeasureUnit GetResult()
            {
                int count = _normM.Count;
                if( count == 0 ) return None;
                if( count == 1 )
                {
                    int exp = _normE[0];
                    return exp != 0 ? RegisterExponent( exp, _normM[0] ) : None;
                }
                var result = new List<ExponentMeasureUnit>( count );
                for( int i = 0; i < count; ++i )
                {
                    int exp = _normE[i];
                    if( exp != 0 ) result.Add( RegisterExponent( exp, _normM[i] ) );
                }
                count = result.Count;
                if( count == 0 ) return None;
                if( count == 1 ) return result[0];
                result.Sort();
                return RegisterCombined( result.ToArray() );
            }
        }

        internal static CombinedMeasureUnit SafeCreate( IEnumerable<ExponentMeasureUnit> units )
        {
            return new Combinator( units ).GetResult();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CK.UnitsOfMeasure
{
    public partial class MeasureUnit
    {
        internal struct Combinator
        {
            readonly List<AtomicMeasureUnit> _normM;
            readonly List<int> _normE;
            ExpFactor _dimensionLessFactor;

            public Combinator( IEnumerable<ExponentMeasureUnit> units )
            {
                _normM = new List<AtomicMeasureUnit>();
                _normE = new List<int>();
                _dimensionLessFactor = ExpFactor.Neutral;
                if( units != null ) Add( units );
            }

            public void Add( IEnumerable<ExponentMeasureUnit> units )
            {
                MeasureContext c = null;
                foreach( var u in units )
                {
                    if( c != u.Context && c != null ) throw new Exception( "Units' Context mismatch." );
                    Add( u.AtomicMeasureUnit, u.Exponent );
                    c = u.Context;
                }
            }

            public void Add( AtomicMeasureUnit u, int exp )
            {
                if( u.AtomicMeasureUnit.Normalization == None )
                {
                    var fp = u.AtomicMeasureUnit.NormalizationFactor.ExpFactor.Power( exp );
                    _dimensionLessFactor = _dimensionLessFactor.Multiply( fp );
                }
                else
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
            }

            public MeasureUnit GetResult( MeasureContext ctx )
            {
                int count = _normM.Count;
                if( count == 0 )
                {
                    if( _dimensionLessFactor.IsNeutral ) return None;
                    return ctx.RegisterPrefixed( _dimensionLessFactor, MeasureStandardPrefix.None, None );
                }
                if( count == 1 && _dimensionLessFactor.IsNeutral )
                {
                    int exp = _normE[0];
                    return exp == 0
                            ? None
                            : (exp == 1
                                ? _normM[0]
                                : ctx.RegisterExponent( exp, _normM[0] ));
                }
                var result = new List<ExponentMeasureUnit>( count );
                for( int i = 0; i < count; ++i )
                {
                    int exp = _normE[i];
                    if( exp != 0 ) result.Add( exp == 1 ? _normM[i] : ctx.RegisterExponent( exp, _normM[i] ) );
                }
                if( !_dimensionLessFactor.IsNeutral )
                {
                    result.Add( ctx.RegisterPrefixed( _dimensionLessFactor, MeasureStandardPrefix.None, None ) );
                }
                count = result.Count;
                if( count == 0 ) return None;
                if( count == 1 ) return result[0];
                result.Sort();
                return ctx.RegisterCombined( result.ToArray() );
            }

            public static MeasureUnit Create( MeasureContext ctx, IEnumerable<ExponentMeasureUnit> units )
            {
                return new Combinator( units ).GetResult( ctx );
            }
        }
    }
}

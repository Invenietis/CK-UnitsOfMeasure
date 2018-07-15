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

            public Combinator( IEnumerable<ExponentMeasureUnit> units )
            {
                _normM = new List<AtomicMeasureUnit>();
                _normE = new List<int>();
                if( units != null ) Add( units );
            }

            public void Add( IEnumerable<ExponentMeasureUnit> units )
            {
                MeasureContext c = null;
                foreach( var u in units )
                {
                    Debug.Assert( u != null && u != None && u.AtomicMeasureUnit != None );
                    if( c != u.Context && c != null ) throw new Exception( "Units' Context mismatch." );
                    Add( u.AtomicMeasureUnit, u.Exponent );
                    c = u.Context;
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

            public MeasureUnit GetResult( MeasureContext ctx )
            {
                int count = _normM.Count;
                if( count == 0 ) return None;
                if( count == 1 )
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

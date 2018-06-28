using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CK.Core
{
    public partial class MeasureUnit
    {
        struct Combinator
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
                foreach( var u in units )
                {
                    Debug.Assert( u != null );
                    if( u.AtomicMeasureUnit != None ) Add( u.AtomicMeasureUnit, u.Exponent );
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

            public MeasureUnit GetResult()
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

            public static MeasureUnit Create( IEnumerable<ExponentMeasureUnit> units )
            {
                return new Combinator( units ).GetResult();
            }
        }
    }
}

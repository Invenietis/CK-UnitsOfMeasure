using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;

namespace CK.Core
{

    /// <summary>
    /// A fundamental measure unit is semantically bound to a dimension, and can be identified by its <see cref="Abbreviation"/>
    /// or by its <see cref="LongName"/>. 
    /// See http://en.wikipedia.org/wiki/SI_base_unit.
    /// </summary>
    public class FundamentalMeasureUnit : AtomicMeasureUnit
    {
        internal FundamentalMeasureUnit( string abbreviation, string name )
            : base( abbreviation, name, true )
        {
        }

        public ExponentMeasureUnit WithExponent( int exp )
        {
            if( exp == 0 ) return None;
            if( exp == 1 ) return this;
            return RegisterExponent( exp, this );
        }

    }
}

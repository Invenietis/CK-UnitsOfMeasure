using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    public class AtomicMeasureUnit : ExponentMeasureUnit, IComparable<AtomicMeasureUnit>
    {
        private protected AtomicMeasureUnit( MeasureContext ctx, string abbreviation, string name, bool isNormalized )
            : base( ctx, abbreviation, name, isNormalized )
        {
        }

        public int CompareTo( AtomicMeasureUnit other )
        {
            return other == null ? 1 : Abbreviation.CompareTo( other.Abbreviation );
        }
    }
}

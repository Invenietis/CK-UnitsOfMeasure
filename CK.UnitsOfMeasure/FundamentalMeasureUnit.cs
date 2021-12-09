using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;

namespace CK.UnitsOfMeasure
{

    /// <summary>
    /// A fundamental measure unit is semantically bound to a dimension, and can be identified
    /// by its <see cref="MeasureUnit.Abbreviation"/>. 
    /// See http://en.wikipedia.org/wiki/SI_base_unit.
    /// </summary>
    public sealed class FundamentalMeasureUnit : AtomicMeasureUnit
    {
        internal FundamentalMeasureUnit( MeasureContext ctx, string abbreviation, string name, AutoStandardPrefix stdPrefix, bool isNormalized )
            : base( ctx, abbreviation, name, stdPrefix, isNormalized )
        {
        }

    }
}

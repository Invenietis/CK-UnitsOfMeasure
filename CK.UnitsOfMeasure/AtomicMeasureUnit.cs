using System;
using System.Collections.Generic;
using System.Text;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// An atomic measure unit generalizes the 3 concrete kind of measures:
    /// <see cref="AliasMeasureUnit"/>, <see cref="PrefixedMeasureUnit"/>
    /// and <see cref="FundamentalMeasureUnit"/>.
    /// </summary>
    public abstract class AtomicMeasureUnit : ExponentMeasureUnit, IComparable<AtomicMeasureUnit>
    {
        private protected AtomicMeasureUnit( MeasureContext ctx,
                                             string abbreviation,
                                             string name,
                                             AutoStandardPrefix stdPrefix,
                                             bool isNormalized )
            : base( ctx, abbreviation, name, isNormalized )
        {
            AutoStandardPrefix = stdPrefix;
        }

        /// <summary>
        /// Gets the <see cref="AutoStandardPrefix"/> configuration of this measure.
        /// </summary>
        public AutoStandardPrefix AutoStandardPrefix { get; }

        /// <summary>
        /// AtomicMeasureUnit are ordered by their <see cref="MeasureUnit.Abbreviation"/>.
        /// </summary>
        /// <param name="other">The other atomic unit. Can be null.</param>
        /// <returns>Standard comparison result (positive, zero or negative).</returns>
        public int CompareTo( AtomicMeasureUnit other )
        {
            return other == null ? 1 : Abbreviation.CompareTo( other.Abbreviation );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// A <see cref="MeasureStandardPrefix"/> applied to a <see cref="AtomicMeasureUnit"/>. 
    /// See http://en.wikipedia.org/wiki/Metric_prefix and https://en.wikipedia.org/wiki/Binary_prefix.
    /// </summary>
    public sealed class PrefixedMeasureUnit : AtomicMeasureUnit
    {
        internal PrefixedMeasureUnit( MeasureContext ctx, (string A, string N) names, ExpFactor adjusment, MeasureStandardPrefix p, AtomicMeasureUnit u, bool isNormalized )
            : base( ctx, names.A, names.N, AutoStandardPrefix.None, isNormalized )
        {
            AdjustmentFactor = adjusment;
            Prefix = p;
            AtomicMeasureUnit = u;
        }

        internal static (string A, string N) ComputeNames( ExpFactor adjustment, MeasureStandardPrefix p, MeasureUnit u )
        {
            if( u == None )
            {
                Debug.Assert( !adjustment.IsNeutral && p == MeasureStandardPrefix.None );
                var name = adjustment.ToString();
                return (name, name);
            }
            if( adjustment.IsNeutral ) return (p.Abbreviation + u.Abbreviation, p.Name + u.Name.ToLowerInvariant());
            string a = '(' + adjustment.ToString() + ')';
            return (a + p.Abbreviation + u.Abbreviation, a + p.Name + u.Name.ToLowerInvariant());
        }

        /// <summary>
        /// Gets the adjustment factor. This is <see cref="ExpFactor.Neutral"/> except when
        /// the <see cref="Prefix"/> can not be resolved exactly (such as when a "DeciGiga" is required)
        /// or when this <see cref="AtomicMeasureUnit.AutoStandardPrefix"/> disallow standard prefixes.
        /// </summary>
        public ExpFactor AdjustmentFactor { get; }

        /// <summary>
        /// Get the standard prefix that applies. 
        /// </summary>
        public MeasureStandardPrefix Prefix { get; }

        /// <summary>
        /// Gets the measure that this <see cref="PrefixedMeasureUnit"/> decorates.
        /// This masks the <see cref="ExponentMeasureUnit.AtomicMeasureUnit"/> property and this is
        /// a valid example of the C# masking language feature.
        /// </summary>
        public new AtomicMeasureUnit AtomicMeasureUnit { get; }

        private protected override (MeasureUnit, FullFactor) GetNormalization()
        {
            return (
                        AtomicMeasureUnit.Normalization,
                        AtomicMeasureUnit.NormalizationFactor
                                        .Multiply( Prefix.Factor )
                                        .Multiply( AdjustmentFactor )
                    );
        }
    }
}

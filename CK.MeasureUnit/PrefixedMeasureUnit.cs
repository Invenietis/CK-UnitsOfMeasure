using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="MeasureStandardPrefix"/> applied to a <see cref="Core.AtomicMeasureUnit"/>. 
    /// See http://en.wikipedia.org/wiki/Metric_prefix and https://en.wikipedia.org/wiki/Binary_prefix.
    /// </summary>
    public sealed class PrefixedMeasureUnit : AtomicMeasureUnit
    {
        internal PrefixedMeasureUnit((string A, string N) names, ExpFactor adjusment, MeasureStandardPrefix p, AtomicMeasureUnit u)
            : base(names.A, names.N )
        {
            Debug.Assert( p != MeasureStandardPrefix.None );
            AdjustmentFactor = adjusment;
            Prefix = p;
            AtomicMeasureUnit = u;
        }

        public static (string A, string N) ComputeNames( ExpFactor adjustment, MeasureStandardPrefix p, MeasureUnit u )
        {
            if( adjustment.IsNeutral ) return (p.Abbreviation + u.Abbreviation, p.Name + u.Name.ToLowerInvariant());
            string a = '(' + adjustment.ToString() + ')';
            return (a + p.Abbreviation + u.Abbreviation, a + p.Name + u.Name.ToLowerInvariant());
        }

        /// <summary>
        /// Gets the adjustment factor. This is <see cref="ExpFactor.Neutral"/> except when
        /// the <see cref="Prefix"/> can not be resolved exactly (such as when a "DeciGiga" is required).
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



    }
}

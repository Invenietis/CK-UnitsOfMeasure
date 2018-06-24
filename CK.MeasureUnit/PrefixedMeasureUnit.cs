using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="MeasureStandardPrefix"/> applied to a <see cref="MeasureUnit"/>. 
    /// See http://en.wikipedia.org/wiki/Metric_prefix and https://en.wikipedia.org/wiki/Binary_prefix.
    /// 
    /// y = x m.s^-2
    /// 1 ms = 10^-3 s <=> 1s = 10^3 ms
    /// y = x.10^-6 m.ms^-2
    /// y = x.10^3 mm.ms^-2
    /// y = x μm.ms-2
    /// 
    /// </summary>
    public sealed class PrefixedMeasureUnit : MeasureUnit
    {

        internal PrefixedMeasureUnit((string A, string L) names, MeasureStandardPrefix p, MeasureUnit u)
            : base(names.A, names.L)
        {
            Prefix = p;
            MeasureUnit = u;
        }

        // Should this "NonPrefixed" be used for aliases like litre "l" => "dm^3" ?
        internal PrefixedMeasureUnit(string abbrevation, string name, MeasureUnit u)
            : base(abbrevation, name)
        {
            Prefix = MeasureStandardPrefix.None;
            MeasureUnit = u;
        }

        public static (string A, string N) ComputeNames(MeasureStandardPrefix p, MeasureUnit u) => (p.Abbreviation + u.Abbreviation, p.Name + " " + u.Name.ToLowerInvariant());

        public MeasureStandardPrefix Prefix { get; }

        public MeasureUnit MeasureUnit { get; }

    }
}

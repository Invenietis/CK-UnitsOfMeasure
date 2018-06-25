using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CK.Core
{
    /// <summary>
    /// An <see cref="AliasMeasureUnit"/> is a <see cref="AtomicMeasureUnit"/> that has its own unique abbreviation and name,
    /// is defined by a <see cref="CombinedMeasureUnit"/> and bound to a normalized <see cref="CombinedMeasureUnit"/>
    /// by a <see cref="FullFactor"/>. 
    /// </summary>
    public sealed class AliasMeasureUnit : AtomicMeasureUnit
    {
        internal AliasMeasureUnit(
            (string A, string L) names,
            FullFactor definitionFactor,
            CombinedMeasureUnit definition,
            FullFactor normalizedFactor,
            CombinedMeasureUnit normalized )
            : base(names.A, names.L)
        {
        }

        public static (string A, string N) ComputeNames(MeasureStandardPrefix p, MeasureUnit u) => (p.Abbreviation + u.Abbreviation, p.Name + " " + u.Name.ToLowerInvariant());

        public FullFactor DefinitionFactor { get; }

        public CombinedMeasureUnit Definition { get; }

        public FullFactor NormalizedFactor { get; }

        public CombinedMeasureUnit NormalizedDefinition { get; }

    }
}

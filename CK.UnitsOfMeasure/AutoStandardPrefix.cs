using System;
using System.Collections.Generic;
using System.Text;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// Defines the automatic support of metric or binary standard prefixes
    /// of a <see cref="AtomicMeasureUnit"/>.
    /// </summary>
    public enum AutoStandardPrefix
    {
        /// <summary>
        /// The unit does not support automatic standard prefixes.
        /// </summary>
        None = 0,

        /// <summary>
        /// The unit supports automatic standard metric prefix (Kilo, Mega, etc.).
        /// </summary>
        Metric = 1,

        /// <summary>
        /// The unit supports automatic standard binary prefix (Kibi, Gibi, etc.).
        /// </summary>
        Binary = 2,

        /// <summary>
        /// The unit automatically support both binary and metric standard
        /// prefix (Kibi, Gibi, as well as Kilo, Mega, etc.).
        /// </summary>
        Both = 3
    }
}

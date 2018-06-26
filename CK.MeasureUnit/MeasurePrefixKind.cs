using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Defines the kind of prefix: metric or binary.
    /// </summary>
    public enum MeasurePrefixKind
    {
        /// <summary>
        /// Non applicable, not a prefix.
        /// </summary>
        None = 0,

        /// <summary>
        /// Metric prefix (Kilo, Mega, etc.). Base is 10.
        /// </summary>
        Metric = 10,

        /// <summary>
        /// Binary preifx (Kibi, Gibi, etc.). Base is 2.
        /// </summary>
        Binary = 2
    }
}

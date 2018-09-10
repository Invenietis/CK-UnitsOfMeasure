using System;
using System.Collections.Generic;
using System.Text;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// Provides extension methods to int and double.
    /// </summary>
    public static class MeasureUnitExtension
    {
        /// <summary>
        /// Constructs a new <see cref="Quantity"/> from an Int32.
        /// </summary>
        /// <param name="this">This Int32.</param>
        /// <param name="u">The unit of the measure of the quantity.</param>
        /// <returns>The quantity.</returns>
        public static Quantity WithUnit( this int @this, MeasureUnit u ) => new Quantity( @this, u );

        /// <summary>
        /// Constructs a new <see cref="Quantity"/> from a double.
        /// </summary>
        /// <param name="this">This double.</param>
        /// <param name="u">The unit of the measure of the quantity.</param>
        /// <returns>The quantity.</returns>
        public static Quantity WithUnit( this double @this, MeasureUnit u ) => new Quantity( @this, u );

    }
}

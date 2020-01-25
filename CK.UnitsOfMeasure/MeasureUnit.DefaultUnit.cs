using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CK.UnitsOfMeasure
{
    public partial class MeasureUnit
    {
        /// <summary>
        /// Tries to parse a string in the default context (<see cref="StandardMeasureContext.Default"/>).
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="u">The resulting measure unit.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool TryParse( string text, out MeasureUnit u ) => StandardMeasureContext.Default.TryParse( text, out u );

        /// <summary>
        /// Dimensionles unit. Associated abbreviation is "" (the empty string) and its name is "None".
        /// Its <see cref="Context"/> is null: the None unit is logically bound to all contexts.
        /// </summary>
        public static FundamentalMeasureUnit None = new FundamentalMeasureUnit( null, "", "None", AutoStandardPrefix.None, true );

        /// <summary>
        /// Dimensionless unit. Used to count items. Associated abbreviation is "#".
        /// </summary>
        public static FundamentalMeasureUnit Unit => StandardMeasureContext.Default.Unit;

        /// <summary>
        /// The metre is the length of the path travelled by light in vacuum during a time interval of 1/299792458 
        /// of a second. 
        /// This is the SI base unit of measure of distance.
        /// Associated abbreviation is "m".
        /// </summary>
        public static FundamentalMeasureUnit Metre => StandardMeasureContext.Default.Metre;

        /// <summary>
        /// The kilogram is the unit of mass; it is equal to the mass of the international prototype of the kilogram.
        /// This is the only SI base unit that includes a prefix. To avoid coping with this exception in the code, we
        /// define it as a <see cref="PrefixedMeasureUnit"/> based on the gram (<see cref="MeasureStandardPrefix.Kilo"/>
        /// of <see cref="Gram"/>). This Kilogram is the normalized unit of mass.
        /// Associated abbreviation is "kg".
        /// </summary>
        public static PrefixedMeasureUnit Kilogram => StandardMeasureContext.Default.Kilogram;

        /// <summary>
        /// The gram is a fundamental unit of mass but not the normalized one: the <see cref="Kilogram"/> is the SI base unit.
        /// Associated abbreviation is "g".
        /// </summary>
        public static FundamentalMeasureUnit Gram => StandardMeasureContext.Default.Gram;

        /// <summary>
        /// The second is the duration of 9192631770 periods of the radiation corresponding to the transition 
        /// between the two hyperfine levels of the ground state of the caesium 133 atom.
        /// This is the SI base unit of measure of time.
        /// Associated abbreviation is "s".
        /// </summary>
        public static FundamentalMeasureUnit Second => StandardMeasureContext.Default.Second;

        /// <summary>
        /// The ampere is that constant current which, if maintained in two straight parallel conductors of 
        /// infinite length, of negligible circular cross-section, and placed 1 metre apart in vacuum, 
        /// would produce between these conductors a force equal to 2×10−7 newton per metre of length.
        /// This is the SI base unit of measure of electric current.
        /// Associated abbreviation is "A".
        /// </summary>
        public static FundamentalMeasureUnit Ampere => StandardMeasureContext.Default.Ampere;

        /// <summary>
        /// The kelvin, unit of thermodynamic temperature, is the fraction 1 / 273.16 of the thermodynamic 
        /// temperature of the triple point of water.
        /// This is the SI base unit of measure of thermodynamic temperature.
        /// Associated abbreviation is "K".
        /// </summary>
        public static FundamentalMeasureUnit Kelvin => StandardMeasureContext.Default.Kelvin;

        /// <summary>
        /// The mole is the amount of substance of a system which contains as many elementary entities 
        /// as there are atoms in 0.012 kilogram of carbon 12; its symbol is 'mol'.
        /// This is the SI base unit of measure of an amount of substance.
        /// Associated abbreviation is "mol".
        /// </summary>
        public static FundamentalMeasureUnit Mole => StandardMeasureContext.Default.Mole;

        /// <summary>
        /// The candela is the luminous intensity, in a given direction, of a source that emits monochromatic 
        /// radiation of frequency 540×1012 hertz and that has a radiant intensity in that direction 
        /// of 1/683 watt per steradian.
        /// This is the SI base unit of measure of luminous intensity.
        /// Associated abbreviation is "cd".
        /// </summary>
        public static FundamentalMeasureUnit Candela => StandardMeasureContext.Default.Candela;

        /// <summary>
        /// A bit is defined as the information entropy of a binary random variable that is 0 or 1 with 
        /// equal probability. 
        /// Associated abbreviation is "b" (recommended by the IEEE 1541-2002 and IEEE Std 260.1-2004 standards). 
        /// </summary>
        public static FundamentalMeasureUnit Bit => StandardMeasureContext.Default.Bit;

        /// <summary>
        /// A byte is now standardized as eight bits, as documented in ISO/IEC 2382-1:1993.
        /// The international standard IEC 80000-13 codified this common meaning.
        /// Associated abbreviation is "B" and it is an alias with a <see cref="ExpFactor"/> of 2^3 on <see cref="Bit"/>.
        /// </summary>
        public static AliasMeasureUnit Byte => StandardMeasureContext.Default.Byte;

    }
}

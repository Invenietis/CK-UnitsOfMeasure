using System;
using System.Collections.Generic;
using System.Text;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// Specialized <see cref="MeasureContext"/> that defines standard units.
    /// </summary>
    public class StandardMeasureContext : MeasureContext
    {
        /// <summary>
        /// Exposes the default context singleton.
        /// </summary>
        public static readonly StandardMeasureContext Default = new StandardMeasureContext( String.Empty, true );

        StandardMeasureContext( string name, bool isDefault )
            : base( name, isDefault )
        {
            Unit = DefineFundamental( "#", "Unit", AutoStandardPrefix.Metric );
            Metre = DefineFundamental( "m", "Metre", AutoStandardPrefix.Metric );
            Gram = DefineFundamental( "g", "Gram", AutoStandardPrefix.Metric, MeasureStandardPrefix.Kilo );
            Second = DefineFundamental( "s", "Second", AutoStandardPrefix.Metric );
            Ampere = DefineFundamental( "A", "Ampere", AutoStandardPrefix.Metric );
            Kelvin = DefineFundamental( "K", "Kelvin", AutoStandardPrefix.Metric );
            Mole = DefineFundamental( "mol", "Mole", AutoStandardPrefix.Metric );
            Candela = DefineFundamental( "cd", "Candela", AutoStandardPrefix.Metric );
            Bit = DefineFundamental( "b", "Bit", AutoStandardPrefix.Both );
            Kilogram = (PrefixedMeasureUnit)this["kg"];
            Byte = DefineAlias( "B", "Byte", new FullFactor( new ExpFactor( 3, 0 ) ), Bit, AutoStandardPrefix.Both );
        }

        /// <summary>
        /// Initializes a new <see cref="StandardMeasureContext"/> with a name
        /// that should uniquely identify this context.
        /// </summary>
        /// <param name="name">Name of this context. Must not be null or empty: the empty name is reserved for <see cref="Default"/>.</param>
        public StandardMeasureContext( string name )
            : this( name, false )
        {
        }

        /// <summary>
        /// Dimensionless unit. Used to count items. Associated abbreviation is "#".
        /// </summary>
        public readonly FundamentalMeasureUnit Unit;

        /// <summary>
        /// The metre is the length of the path travelled by light in vacuum during a time interval of 1/299792458 
        /// of a second. 
        /// This is the SI base unit of measure of distance.
        /// Associated abbreviation is "m".
        /// </summary>
        public readonly FundamentalMeasureUnit Metre;

        /// <summary>
        /// The kilogram is the unit of mass; it is equal to the mass of the international prototype of the kilogram.
        /// This is the only SI base unit that incluedes a prefix. To avoid coping with this exception in the code, we
        /// define it as a <see cref="PrefixedMeasureUnit"/> based on the gram (<see cref="MeasureStandardPrefix.Kilo"/>
        /// of <see cref="Gram"/>) .
        /// Associated abbreviation is "kg".
        /// </summary>
        public readonly PrefixedMeasureUnit Kilogram;

        /// <summary>
        /// The gram is our fundamental unit of mass (see <see cref="Kilogram"/>).
        /// Associated abbreviation is "g".
        /// </summary>
        public readonly FundamentalMeasureUnit Gram;

        /// <summary>
        /// The second is the duration of 9192631770 periods of the radiation corresponding to the transition 
        /// between the two hyperfine levels of the ground state of the caesium 133 atom.
        /// This is the SI base unit of measure of time.
        /// Associated abbreviation is "s".
        /// </summary>
        public readonly FundamentalMeasureUnit Second;

        /// <summary>
        /// The ampere is that constant current which, if maintained in two straight parallel conductors of 
        /// infinite length, of negligible circular cross-section, and placed 1 metre apart in vacuum, 
        /// would produce between these conductors a force equal to 2×10−7 newton per metre of length.
        /// This is the SI base unit of measure of electric current.
        /// Associated abbreviation is "A".
        /// </summary>
        public readonly FundamentalMeasureUnit Ampere;

        /// <summary>
        /// The kelvin, unit of thermodynamic temperature, is the fraction 1 / 273.16 of the thermodynamic 
        /// temperature of the triple point of water.
        /// This is the SI base unit of measure of thermodynamic temperature.
        /// Associated abbreviation is "K".
        /// </summary>
        public readonly FundamentalMeasureUnit Kelvin;

        /// <summary>
        /// The mole is the amount of substance of a system which contains as many elementary entities 
        /// as there are atoms in 0.012 kilogram of carbon 12; its symbol is 'mol'.
        /// This is the SI base unit of measure of an amount of substance.
        /// Associated abbreviation is "mol".
        /// </summary>
        public readonly FundamentalMeasureUnit Mole;

        /// <summary>
        /// The candela is the luminous intensity, in a given direction, of a source that emits monochromatic 
        /// radiation of frequency 540×1012 hertz and that has a radiant intensity in that direction 
        /// of 1/683 watt per steradian.
        /// This is the SI base unit of measure of luminous intensity.
        /// Associated abbreviation is "cd".
        /// </summary>
        public readonly FundamentalMeasureUnit Candela;

        /// <summary>
        /// A bit is defined as the information entropy of a binary random variable that is 0 or 1 with 
        /// equal probability. 
        /// Associated abbreviation is "b" (recommended by the IEEE 1541-2002 and IEEE Std 260.1-2004 standards). 
        /// </summary>
        public readonly FundamentalMeasureUnit Bit;

        /// <summary>
        /// A byte is now standardized as eight bits, as documented in ISO/IEC 2382-1:1993.
        /// The international standard IEC 80000-13 codified this common meaning.
        /// Associated abbreviation is "B" and it is an alias with a <see cref="ExpFactor"/> of 2^3 on <see cref="Bit"/>.
        /// </summary>
        public readonly AliasMeasureUnit Byte;

    }
}

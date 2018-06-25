using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CK.Core
{
    /// <summary>
    /// Base class for all measure unit. 
    /// </summary>
    public abstract class MeasureUnit
    {
        static readonly ConcurrentDictionary<string, MeasureUnit> _units;

        /// <summary>
        /// Dimensionles unit. Associated abbreviation is "" (the empty string) and its name is "None".
        /// </summary>
        public static readonly FundamentalMeasureUnit None;

        /// <summary>
        /// Dimensionless unit. Used to count items. Associated abbreviation is "#".
        /// </summary>
        public static readonly FundamentalMeasureUnit Unit;

        /// <summary>
        /// The metre is the length of the path travelled by light in vacuum during a time interval of 1/299792458 
        /// of a second. 
        /// This is the SI base unit of measure of distance.
        /// Associated abbreviation is "m".
        /// </summary>
        public static readonly FundamentalMeasureUnit Metre;

        /// <summary>
        /// The kilogram is the unit of mass; it is equal to the mass of the international prototype of the kilogram.
        /// This is the SI base unit of measure of weight.
        /// Associated abbreviation is "kg".
        /// </summary>
        public static readonly FundamentalMeasureUnit Kilogram;

        /// <summary>
        /// The second is the duration of 9192631770 periods of the radiation corresponding to the transition 
        /// between the two hyperfine levels of the ground state of the caesium 133 atom.
        /// This is the SI base unit of measure of time.
        /// Associated abbreviation is "s".
        /// </summary>
        public static readonly FundamentalMeasureUnit Second;

        /// <summary>
        /// The ampere is that constant current which, if maintained in two straight parallel conductors of 
        /// infinite length, of negligible circular cross-section, and placed 1 metre apart in vacuum, 
        /// would produce between these conductors a force equal to 2×10−7 newton per metre of length.
        /// This is the SI base unit of measure of electric current.
        /// Associated abbreviation is "A".
        /// </summary>
        public static readonly FundamentalMeasureUnit Ampere;

        /// <summary>
        /// The kelvin, unit of thermodynamic temperature, is the fraction 1 / 273.16 of the thermodynamic 
        /// temperature of the triple point of water.
        /// This is the SI base unit of measure of thermodynamic temperature.
        /// Associated abbreviation is "K".
        /// </summary>
        public static readonly FundamentalMeasureUnit Kelvin;

        /// <summary>
        /// The mole is the amount of substance of a system which contains as many elementary entities 
        /// as there are atoms in 0.012 kilogram of carbon 12; its symbol is 'mol'.
        /// This is the SI base unit of measure of an amount of substance.
        /// Associated abbreviation is "mol".
        /// </summary>
        public static readonly FundamentalMeasureUnit Mole;

        /// <summary>
        /// The candela is the luminous intensity, in a given direction, of a source that emits monochromatic 
        /// radiation of frequency 540×1012 hertz and that has a radiant intensity in that direction 
        /// of 1/683 watt per steradian.
        /// This is the SI base unit of measure of luminous intensity.
        /// Associated abbreviation is "cd".
        /// </summary>
        public static readonly FundamentalMeasureUnit Candela;

        /// <summary>
        /// A bit is defined as the information entropy of a binary random variable that is 0 or 1 with 
        /// equal probability. 
        /// Associated abbreviation is "b" (recommended by the IEEE 1541-2002 and IEEE Std 260.1-2004 standards). 
        /// </summary>
        public static readonly FundamentalMeasureUnit Bit;

        ///// <summary>
        ///// A byte is now standardized as eight bits, as documented in ISO/IEC 2382-1:1993.
        ///// The international standard IEC 80000-13 codified this common meaning.
        ///// Associated trait is "B".
        ///// </summary>
        //public static readonly CKTrait Byte;

        static MeasureUnit()
        {
            None = new FundamentalMeasureUnit( "", "None" );
            Unit = new FundamentalMeasureUnit( "#", "Unit" );
            Metre = new FundamentalMeasureUnit( "m", "Metre" );
            Kilogram = new FundamentalMeasureUnit( "kg", "Kilogram" );
            Second = new FundamentalMeasureUnit( "s", "Second" );
            Ampere = new FundamentalMeasureUnit( "A", "Ampere" );
            Kelvin = new FundamentalMeasureUnit( "K", "Kelvin" );
            Mole = new FundamentalMeasureUnit( "mol", "Mole" );
            Candela = new FundamentalMeasureUnit( "cd", "Candela" );
            Bit = new FundamentalMeasureUnit( "b", "Bit" );

            var basics = new[]
            {
                new KeyValuePair<string,MeasureUnit>( None.Abbreviation, None ),
                new KeyValuePair<string,MeasureUnit>( Unit.Abbreviation, Unit ),
                new KeyValuePair<string,MeasureUnit>( Metre.Abbreviation, Metre ),
                new KeyValuePair<string,MeasureUnit>( Kilogram.Abbreviation, Kilogram ),
                new KeyValuePair<string,MeasureUnit>( Second.Abbreviation, Second ),
                new KeyValuePair<string,MeasureUnit>( Ampere.Abbreviation, Ampere ),
                new KeyValuePair<string,MeasureUnit>( Kelvin.Abbreviation, Kelvin ),
                new KeyValuePair<string,MeasureUnit>( Mole.Abbreviation, Mole ),
                new KeyValuePair<string,MeasureUnit>( Candela.Abbreviation, Candela ),
                new KeyValuePair<string,MeasureUnit>( Bit.Abbreviation, Bit ),
            };
            // Case sensitive is mandatory (mSv is not MSv - Milli vs. Mega).
            _units = new ConcurrentDictionary<string, MeasureUnit>( basics );
        }


        protected MeasureUnit( string abbreviation, string name )
        {
            Abbreviation = abbreviation;
            Name = name;
        }

        public string Abbreviation { get; }

        public string Name { get; }

        public string ToString( bool withName ) => withName ? $"{Abbreviation} ({Name})" : Abbreviation;

        public override string ToString() => Abbreviation;

        static Regex _rUnit = new Regex( @"(?<1>[^\.\^]+)(\^(?<2>-?\d+))?", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        //public static MeasureUnit Parse( string s )
        //{
        //    // 1 - Normalize input by removing all white spaces.
        //    // Trust the Regex cache for this quite common one.
        //    s = Regex.Replace(s, "\\s+", String.Empty);

        //    // 2 - Obtain the list of Abbreviation[^Exp].
        //    (string Abb, int Exp) Map( Match m )
        //    {
        //        var num = m.Captures[2].Value;
        //        int e = num.Length > 0 ? Int32.Parse(num) : 1;
        //        return (m.Captures[1].Value, e );
        //    }

        //    _rUnit.Matches( s ).Cast<Match>().Select( Map )

        //    m.MatchWord()
        //    MeasureUnit result = null;
        //    var parts = s.Split('.');
        //    if( s.Length == 1 )
        //    {
        //        if (_units.TryGetValue(s, out result)) return result;
        //        var m = new StringMatcher(s);
        //        int idx = s.IndexOfAny(new[] { '^', '²' });
        //        if (idx <= 0) return null;
        //        if( )
        //    }
        //}

        private protected static CombinedMeasureUnit RegisterCombined( ExponentMeasureUnit[] units)
        {
            var names = CombinedMeasureUnit.ComputeNames(units);
            return Register(names.A, names.N, () => new CombinedMeasureUnit(names,units), null);
        }

        private protected static ExponentMeasureUnit RegisterExponent(int exp, AtomicMeasureUnit u)
        {
            var names = ExponentMeasureUnit.ComputeNames(exp, u);
            return Register(names.A, names.N, () => new ExponentMeasureUnit(names, exp, u), null);
        }

        static internal PrefixedMeasureUnit RegisterPrefixed(ExpFactor adjustment, MeasureStandardPrefix p, AtomicMeasureUnit u)
        {
            var names = PrefixedMeasureUnit.ComputeNames( adjustment, p, u);
            return Register(names.A, names.N, () => new PrefixedMeasureUnit(names, adjustment, p, u), null);
        }

        static FundamentalMeasureUnit RegisterFundamental(string abbreviation, string name)
        {
            return Register( abbreviation, name, () => new FundamentalMeasureUnit(abbreviation,name), null );
        }

        static T Register<T>(string abbreviation, string name, Func<T> creator, Func<T, bool> checker )
                    where T : MeasureUnit
        {
            T m = CheckSingleRegistration(abbreviation, name, checker);
            if (m == null)
            {
                var newOne = creator();
                _units.GetOrAdd(abbreviation, newOne);
                m = CheckSingleRegistration(abbreviation, name, checker );
            }
            return m;
        }

        static T CheckSingleRegistration<T>(string abbreviation, string name, Func<T,bool> check ) 
            where T : MeasureUnit
        {
            T result = null;
            // If they are not the same (null included) or if the type is not the right one or 
            // there is a check function that fails, this an error.
            if( _units.TryGetValue(abbreviation, out var m)
                && 
                (m.Abbreviation != abbreviation
                 || m.Name != name
                 || (result = m as T) == null
                 || (m != null && check != null && !check(result)) ) )
            {
                throw new Exception( $"Registration mismatch for {abbreviation} ({name}).");
            }
            return result;
        }
    }
}

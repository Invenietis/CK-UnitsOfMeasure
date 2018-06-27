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
    public abstract partial class MeasureUnit
    {
        static readonly ConcurrentDictionary<string, MeasureUnit> _units;

        static MeasureUnit()
        {
            None = new FundamentalMeasureUnit( "", "None" );
            Unit = new FundamentalMeasureUnit( "#", "Unit" );
            Metre = new FundamentalMeasureUnit( "m", "Metre" );
            Gram = new FundamentalMeasureUnit( "g", "Gram" );
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
                new KeyValuePair<string,MeasureUnit>( Gram.Abbreviation, Gram ),
                new KeyValuePair<string,MeasureUnit>( Second.Abbreviation, Second ),
                new KeyValuePair<string,MeasureUnit>( Ampere.Abbreviation, Ampere ),
                new KeyValuePair<string,MeasureUnit>( Kelvin.Abbreviation, Kelvin ),
                new KeyValuePair<string,MeasureUnit>( Mole.Abbreviation, Mole ),
                new KeyValuePair<string,MeasureUnit>( Candela.Abbreviation, Candela ),
                new KeyValuePair<string,MeasureUnit>( Bit.Abbreviation, Bit ),
            };
            // Case sensitivity is mandatory (mSv is not MSv - Milli vs. Mega).
            _units = new ConcurrentDictionary<string, MeasureUnit>( basics );
            Kilogram = RegisterPrefixed( ExpFactor.Neutral, MeasureStandardPrefix.Kilo, Gram );
            Byte = new AliasMeasureUnit( "B", "Byte", new FullFactor( new ExpFactor( 3, 0 ) ), Bit );
        }


        /// <summary>
        /// Defines an alias.
        /// The same alias can be registered multiple times but it has to exactly match the previously registered one.
        /// </summary>
        /// <param name="abbreviation">
        /// The unit of measure abbreviation.
        /// This is the key that is used. It must not be null or empty.
        /// </param>
        /// <param name="name">The full name. Must not be null or empty.</param>
        /// <param name="definitionFactor">
        /// The factor that applies to the <see cref="AliasMeasureUnit.Definition"/>.
        /// Must not be <see cref="FullFactor.Zero"/>.
        /// </param>
        /// <param name="definition">The definition. Can be any <see cref="CombinedMeasureUnit"/>.</param>
        /// <returns>The alias unit of measure.</returns>
        public static AliasMeasureUnit DefineAlias( string abbreviation, string name, FullFactor definitionFactor, CombinedMeasureUnit definition )
        {
            if( string.IsNullOrWhiteSpace( abbreviation ) ) throw new ArgumentException( "Must not be null or white space.", nameof( abbreviation ) );
            if( string.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Must not be null or white space.", nameof( name ) );
            if( definitionFactor.IsZero ) throw new ArgumentException( "Must not be zero.", nameof( definitionFactor ) );
            if( definition == null ) throw new ArgumentNullException( nameof( definition ) );
            return RegisterAlias( abbreviation, name, definitionFactor, definition );
        }

        /// <summary>
        /// Define a new fundamental unit of measure.
        /// Just like <see cref="DefineAlias(string, string, FullFactor, CombinedMeasureUnit)"/>, the same fundamental unit
        /// can be redefined multiple times as long as it is actually the same: for fundamental units, the <see cref="Name"/>
        /// must be exaclty the same.
        /// </summary>
        /// <param name="abbreviation">
        /// The unit of measure abbreviation.
        /// This is the key that is used. It must not be null or empty.
        /// </param>
        /// <param name="name">The full name. Must not be null or empty.</param>
        /// <returns>The alias unit of measure.</returns>
        public static FundamentalMeasureUnit DefineFundamental( string abbreviation, string name )
        {
            if( string.IsNullOrWhiteSpace( abbreviation ) ) throw new ArgumentException( "Must not be null or white space.", nameof( abbreviation ) );
            if( string.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Must not be null or white space.", nameof( name ) );
            return RegisterFundamental( abbreviation, name );
        }

        static AliasMeasureUnit RegisterAlias( string a, string n, FullFactor f, CombinedMeasureUnit d )
        {
            return Register( a, n, () => new AliasMeasureUnit( a, n, f, d ), m => m.DefinitionFactor == f && m.Definition == d );
        }

        internal static CombinedMeasureUnit RegisterCombined( ExponentMeasureUnit[] units )
        {
            var names = CombinedMeasureUnit.ComputeNames( units );
            return Register( names.A, names.N, () => new CombinedMeasureUnit( names, units ), null );
        }

        internal static ExponentMeasureUnit RegisterExponent(int exp, AtomicMeasureUnit u)
        {
            var names = ExponentMeasureUnit.ComputeNames(exp, u);
            return Register(names.A, names.N, () => new ExponentMeasureUnit(names, exp, u), null);
        }

        internal static PrefixedMeasureUnit RegisterPrefixed(ExpFactor adjustment, MeasureStandardPrefix p, AtomicMeasureUnit u)
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

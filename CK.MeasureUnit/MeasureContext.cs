using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.Core
{
    public class MeasureContext
    {
        readonly ConcurrentDictionary<string, MeasureUnit> _allUnits;

        public MeasureContext()
        {
            _allUnits = new ConcurrentDictionary<string, MeasureUnit>();
        }

        /// <summary>
        /// Gets a <see cref="MeasureUnit"/> from its abbreviation or null if it has not been registered.
        /// </summary>
        /// <param name="abbreviation">The abbreviation.</param>
        /// <returns>The measure unit or null.</returns>
        public MeasureUnit this[string abbreviation]
        {
            get
            {
                _allUnits.TryGetValue( abbreviation, out var u );
                return u;
            }
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
        /// <param name="definition">The definition. Can be any <see cref="MeasureUnit"/>.</param>
        /// <returns>The alias unit of measure.</returns>
        public AliasMeasureUnit DefineAlias( string abbreviation, string name, FullFactor definitionFactor, MeasureUnit definition )
        {
            if( string.IsNullOrWhiteSpace( abbreviation ) ) throw new ArgumentException( "Must not be null or white space.", nameof( abbreviation ) );
            if( string.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Must not be null or white space.", nameof( name ) );
            if( definitionFactor.IsZero ) throw new ArgumentException( "Must not be zero.", nameof( definitionFactor ) );
            if( definition == null ) throw new ArgumentNullException( nameof( definition ) );
            if( definition.Context != this ) throw new Exception( "Units' Context mismatch." );
            return RegisterAlias( abbreviation, name, definitionFactor, definition );
        }

        /// <summary>
        /// Define a new fundamental unit of measure.
        /// Just like <see cref="DefineAlias(string, string, FullFactor, MeasureUnit)"/>, the same fundamental unit
        /// can be redefined multiple times as long as it is actually the same: for fundamental units, the <see cref="Name"/>
        /// (and the normalizedPrefix if any) must be exaclty the same.
        /// </summary>
        /// <param name="abbreviation">
        /// The unit of measure abbreviation.
        /// This is the key that is used. It must not be null or empty.
        /// </param>
        /// <param name="name">The full name. Must not be null or empty.</param>
        /// <param name="normalizedPrefix">
        /// Optional prefix to be used for units where the normalized unit should not be the <see cref="FundamentalMeasureUnit"/> but one of its
        /// <see cref="PrefixedMeasureUnit"/>. This is the case for the "g"/"Gram" and the "kg"/"Kilogram".
        /// Defaults to <see cref="MeasureStandardPrefix.None"/>: by default a fundamental unit is the normalized one.
        /// </param>
        /// <returns>The fundamental unit of measure.</returns>
        public FundamentalMeasureUnit DefineFundamental( string abbreviation, string name, MeasureStandardPrefix normalizedPrefix = null )
        {
            if( string.IsNullOrWhiteSpace( abbreviation ) ) throw new ArgumentException( "Must not be null or white space.", nameof( abbreviation ) );
            if( string.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Must not be null or white space.", nameof( name ) );
            if( normalizedPrefix == null || normalizedPrefix == MeasureStandardPrefix.None )
            {
                return RegisterFundamental( abbreviation, name, true );
            }
            var f = RegisterFundamental( abbreviation, name, false );
            RegisterPrefixed( ExpFactor.Neutral, normalizedPrefix, f, true );
            return f;
        }

        AliasMeasureUnit RegisterAlias( string a, string n, FullFactor f, MeasureUnit d )
        {
            return Register( abbreviation: a,
                                name: n,
                                creator: () => new AliasMeasureUnit( this, a, n, f, d ),
                                checker: m => m.Definition.Normalization == d.Normalization
                                                && m.NormalizationFactor == d.NormalizationFactor.Multiply( f ) );
        }

        internal MeasureUnit RegisterCombined( ExponentMeasureUnit[] units )
        {
            Debug.Assert( units.Length > 0 && units.All( m => m != null ) && units.Length > 1 );
            var names = (String.Join( ".", units.Select( u => u.Abbreviation ) ), String.Join( ".", units.Select( u => u.Name ) ));
            return Register( names.Item1, names.Item2, () => new MeasureUnit( this, names, units ), null );
        }

        internal ExponentMeasureUnit RegisterExponent( int exp, AtomicMeasureUnit u )
        {
            var names = ExponentMeasureUnit.ComputeNames( exp, u );
            return Register( names.A, names.N, () => new ExponentMeasureUnit( this, names, exp, u ), null );
        }

        /// <summary>
        /// The isNormalized is nullable: when automatically creating PrefixedUnit this is null and :
        ///  - if the unit must be created it won't be the normalized one.
        ///  - if the unit is found, we don't check the potential race condition.
        /// Only when explicitly registering/creating the unit with true or false, the resulting unit IsNormalized
        /// property is checked to prevent race conditions.
        /// </summary>
        internal PrefixedMeasureUnit RegisterPrefixed( ExpFactor adjustment, MeasureStandardPrefix p, AtomicMeasureUnit u, bool? isNormalized = null )
        {
            var names = PrefixedMeasureUnit.ComputeNames( adjustment, p, u );
            return Register( names.A, names.N, () => new PrefixedMeasureUnit( this, names, adjustment, p, u, isNormalized ?? false ), m => !isNormalized.HasValue || m.IsNormalized == isNormalized );
        }

        FundamentalMeasureUnit RegisterFundamental( string abbreviation, string name, bool isNormalized )
        {
            return Register( abbreviation, name, () => new FundamentalMeasureUnit( this, abbreviation, name, isNormalized ), m => m.IsNormalized == isNormalized );
        }

        T Register<T>( string abbreviation, string name, Func<T> creator, Func<T, bool> checker )
                    where T : MeasureUnit
        {
            T m = CheckSingleRegistration( abbreviation, name, checker );
            if( m == null )
            {
                var newOne = creator();
                _allUnits.GetOrAdd( abbreviation, newOne );
                m = CheckSingleRegistration( abbreviation, name, checker );
            }
            return m;
        }

        T CheckSingleRegistration<T>( string abbreviation, string name, Func<T, bool> check )
            where T : MeasureUnit
        {
            T result = null;
            // If they are not the same (null included) or if the type is not the right one or 
            // there is a check function that fails, this an error.
            if( _allUnits.TryGetValue( abbreviation, out var m )
                &&
                (m.Abbreviation != abbreviation
                 || m.Name != name
                 || (result = m as T) == null
                 || (m != null && check != null && !check( result ))) )
            {
                throw new Exception( $"Registration mismatch for {abbreviation} ({name})." );
            }
            return result;
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// A MeasureContext manages a set of <see cref="MeasureUnit"/>.
    /// </summary>
    public partial class MeasureContext
    {
        readonly ConcurrentDictionary<string, MeasureUnit> _allUnits;

        internal MeasureContext( string name, bool isDefault )
        {
            _allUnits = new ConcurrentDictionary<string, MeasureUnit>( StringComparer.Ordinal );
            if( !isDefault )
            {
                if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Must not be null or empty.", nameof( name ) );
            }
            Name = name;
        }

        /// <summary>
        /// Creates an empty <see cref="MeasureContext"/> with a name
        /// that should uniquely identify this context.
        /// </summary>
        /// <param name="name">
        /// Name of this context. Must not be null or empty: the empty name is reserved
        /// for the <see cref="StandardMeasureContext.Default"/> default singleton context.
        /// </param>
        public MeasureContext( string name )
            : this( name, false )
        {
        }

        /// <summary>
        /// Gets the name of this context.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a <see cref="MeasureUnit"/> from its abbreviation or null if it has not been registered.
        /// Abbreviations are case sensitive ('b' for bit must coexist with 'B' for byte).
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
        /// Qualifies the abbreviation potential clash detected by <see cref="CheckValidNewAbbreviation(string, AutoStandardPrefix)"/>.
        /// </summary>
        public enum NewAbbreviationConflict
        {
            /// <summary>
            /// No conflict at all.
            /// </summary>
            None,

            /// <summary>
            /// The abbreviation already exists. If redefined, the redefinition must match the existing definition.
            /// </summary>
            Exists,

            /// <summary>
            /// The abbreviation contains invalid characters (or is empty).
            /// </summary>
            InvalidCharacters,

            /// <summary>
            /// At least one automatic prefix of this unit will clash with a current registered unit.
            /// (This can never happen when <see cref="AutoStandardPrefix.None"/> is used to check
            /// the abbreviation).
            /// </summary>
            AmbiguousAutoStandardPrefix,

            /// <summary>
            /// The abbreviation will clash with an automatically standard prefixed unit.
            /// </summary>
            MatchPotentialAutoStandardPrefixedUnit
        }

        /// <summary>
        /// Checks whether a new unit abbreviation can be defined (or may be redefined) in this context.
        /// </summary>
        /// <param name="a">The proposed abbreviation.</param>
        /// <param name="autoStandardPrefix">
        /// Whether the new unit must support automatic metric and/or binary standard prefixes.
        /// </param>
        /// <returns>The <see cref="NewAbbreviationConflict"/> flag.</returns>
        public NewAbbreviationConflict CheckValidNewAbbreviation( string a, AutoStandardPrefix autoStandardPrefix )
        {
            if( _allUnits.ContainsKey( a ) )
            {
                return NewAbbreviationConflict.Exists;
            }

            if( String.IsNullOrEmpty( a )
                || !a.All( c => Char.IsLetter( c )
                                || Char.IsSymbol( c )
                                || c == '#'
                                || c == '%' || c == '‰' || c == '‱'
                                || c == '㏙' ) )
            {
                return NewAbbreviationConflict.InvalidCharacters;
            }
            foreach( var withPrefix in MeasureStandardPrefix.GetPrefixes( autoStandardPrefix )
                                                            .Select( p => p.Abbreviation + a ) )
            {
                if( _allUnits.ContainsKey( withPrefix ) ) return NewAbbreviationConflict.AmbiguousAutoStandardPrefix;
            }
            // Optimization: if the new abbreviation does not start with a
            // standard prefix, it is useless to challenge it against
            // existing units.
            var prefix = MeasureStandardPrefix.FindPrefix( a );
            if( prefix != MeasureStandardPrefix.None
                && _allUnits.Values
                            .OfType<AtomicMeasureUnit>()
                            .Where( u => u.AutoStandardPrefix != AutoStandardPrefix.None )
                            .SelectMany( u => MeasureStandardPrefix.GetPrefixes( u.AutoStandardPrefix )
                                                .Where( p => p != MeasureStandardPrefix.None )
                                                .Select( p => p.Abbreviation + u.Abbreviation ) )
                            .Any( exists => exists == a ) )
            {
                return NewAbbreviationConflict.MatchPotentialAutoStandardPrefixedUnit;
            }
            return NewAbbreviationConflict.None;
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
        /// <param name="autoStandardPrefix">
        /// Whether standard metric and/or binary prefixes can be applied to the unit.
        /// </param>
        /// <returns>The alias unit of measure.</returns>
        public AliasMeasureUnit DefineAlias(
            string abbreviation,
            string name,
            FullFactor definitionFactor,
            MeasureUnit definition,
            AutoStandardPrefix autoStandardPrefix = AutoStandardPrefix.None )
        {
            CheckArgumentAbbreviation( abbreviation, autoStandardPrefix );
            if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Must not be null or white space.", nameof( name ) );
            if( definitionFactor.IsZero ) throw new ArgumentException( "Must not be zero.", nameof( definitionFactor ) );
            if( definition == null ) throw new ArgumentNullException( nameof( definition ) );
            if( definition != MeasureUnit.None && definition.Context != this ) throw new Exception( "Units' Context mismatch." );
            return RegisterAlias( abbreviation, name, definitionFactor, definition, autoStandardPrefix );
        }

        /// <summary>
        /// Define a new fundamental unit of measure.
        /// Just like <see cref="DefineAlias"/>, the same fundamental unit can be redefined multiple times
        /// as long as it is actually the same: for fundamental units, the <see cref="Name"/>
        /// (and the normalizedPrefix if any) must be exactly the same.
        /// </summary>
        /// <param name="abbreviation">
        /// The unit of measure abbreviation.
        /// This is the key that is used. It must not be null or empty.
        /// </param>
        /// <param name="name">The full name. Must not be null or empty.</param>
        /// <param name="autoStandardPrefix">
        /// Whether standard metric and/or binary prefixes can be applied to the unit.
        /// If a <paramref name="normalizedPrefix"/> is used, this must necessarily define the
        /// corresponding prefix kind.
        /// </param>
        /// <param name="normalizedPrefix">
        /// Optional prefix to be used for units where the normalized unit should not be the <see cref="FundamentalMeasureUnit"/> but one of its
        /// <see cref="PrefixedMeasureUnit"/>. This is the case for the "g"/"Gram" and the "kg"/"Kilogram".
        /// Defaults to <see cref="MeasureStandardPrefix.None"/>: by default a fundamental unit is the normalized one.
        /// </param>
        /// <returns>The fundamental unit of measure.</returns>
        public FundamentalMeasureUnit DefineFundamental(
            string abbreviation,
            string name,
            AutoStandardPrefix autoStandardPrefix = AutoStandardPrefix.None,
            MeasureStandardPrefix normalizedPrefix = null )
        {
            CheckArgumentAbbreviation( abbreviation, autoStandardPrefix );
            if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Must not be null or white space.", nameof( name ) );
            if( normalizedPrefix == null || normalizedPrefix == MeasureStandardPrefix.None )
            {
                return RegisterFundamental( abbreviation, name, autoStandardPrefix, true );
            }
            if( normalizedPrefix.Base == 2 && (autoStandardPrefix & AutoStandardPrefix.Binary) == 0 )
            {
                throw new ArgumentException( $"Since the normalization is {normalizedPrefix.Name}, {abbreviation} unit must support AutoStandardPrefix.Binary.", nameof( autoStandardPrefix ) );
            }
            if( normalizedPrefix.Base == 10 && (autoStandardPrefix & AutoStandardPrefix.Metric) == 0 )
            {
                throw new ArgumentException( $"Since the normalization is {normalizedPrefix.Name}, {abbreviation} unit must support AutoStandardPrefix.Metric.", nameof( autoStandardPrefix ) );
            }
            var f = RegisterFundamental( abbreviation, name, autoStandardPrefix, false );
            f.SetPrefixedNormalization( RegisterPrefixed( ExpFactor.Neutral, normalizedPrefix, f, true ) );
            return f;
        }

        AliasMeasureUnit RegisterAlias( string a, string n, FullFactor f, MeasureUnit d, AutoStandardPrefix stdPrefix )
        {
            if( d == MeasureUnit.None )
            {
                if( f.Factor != 1.0 ) throw new ArgumentException( "Only exponential factor is allowed for dimensionless units." );
                d = RegisterPrefixed( f.ExpFactor, MeasureStandardPrefix.None, MeasureUnit.None, false );
                f = FullFactor.Neutral;
            }
            return Register( abbreviation: a,
                             name: n,
                             creator: () => new AliasMeasureUnit( this, a, n, f, d, stdPrefix ),
                             checker: m =>
                             {
                                 if( m.Definition.Normalization != d.Normalization ) ThrowArgumentException( m, $"new definition unit '{d}' is not compatible with '{m.Definition}'." );
                                 if( m.NormalizationFactor != d.NormalizationFactor.Multiply( f ) ) ThrowArgumentException( m, $"new normalization factor '{f}' should be '{m.NormalizationFactor.DivideBy(d.NormalizationFactor)}'." );
                                 CheckArgumentAutoStdPrefix( m, stdPrefix );
                             } );
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
        /// The isNormalized is nullable: when automatically creating PrefixedUnit this is null and:
        ///  - if the unit must be created it won't be the normalized one.
        ///  - if the unit is found, we don't check the potential race condition.
        /// Only when explicitly registering/creating the unit with true or false, the resulting unit IsNormalized
        /// property is checked to prevent race conditions.
        /// </summary>
        internal PrefixedMeasureUnit RegisterPrefixed( ExpFactor adjustment, MeasureStandardPrefix p, AtomicMeasureUnit u, bool? isNormalized = null )
        {
            var names = PrefixedMeasureUnit.ComputeNames( adjustment, p, u );
            return Register( names.A, names.N, () => new PrefixedMeasureUnit( this, names, adjustment, p, u, isNormalized ?? false ), m =>
            {
                if( isNormalized.HasValue && m.IsNormalized != isNormalized ) ThrowArgumentException( m, $"new IsNormalized '{isNormalized}' differ from '{m.IsNormalized}'." );
            } );
        }

        FundamentalMeasureUnit RegisterFundamental( string abbreviation, string name, AutoStandardPrefix stdPrefix, bool isNormalized )
        {
            return Register( abbreviation, name,
                             creator: () => new FundamentalMeasureUnit( this, abbreviation, name, stdPrefix, isNormalized ),
                             checker: m =>
                             {
                                 if( m.IsNormalized != isNormalized ) ThrowArgumentException( m, $"new IsNormalized {isNormalized} differ from {m.IsNormalized}." );
                                 CheckArgumentAutoStdPrefix( m, stdPrefix );
                             } );
        }

        T Register<T>( string abbreviation, string name, Func<T> creator, Action<T> checker )
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

        T CheckSingleRegistration<T>( string abbreviation, string name, Action<T> check )
            where T : MeasureUnit
        {
            T result = null;
            if( _allUnits.TryGetValue( abbreviation, out var m ) )
            {
                Debug.Assert( m.Abbreviation == abbreviation );
                if( m.Name != name ) ThrowArgumentException( m, $"new name '{name}' differ from '{m.Name}'." );
                if( (result = m as T) == null ) ThrowArgumentException( m, $"new type '{typeof( T ).Name}' differ from '{m.GetType().Name}'." );
                check?.Invoke( result );
            }
            return result;
        }

        void CheckArgumentAbbreviation( string abbreviation, AutoStandardPrefix autoStandardPrefix )
        {
            var conflict = CheckValidNewAbbreviation( abbreviation, autoStandardPrefix );
            if( conflict != NewAbbreviationConflict.None && conflict != NewAbbreviationConflict.Exists )
            {
                throw new ArgumentException( $"Invalid abbreviation {abbreviation} ({conflict}).", nameof( abbreviation ) );
            }
        }


        void CheckArgumentAutoStdPrefix( AtomicMeasureUnit m, AutoStandardPrefix stdPrefix )
        {
            if( m.AutoStandardPrefix != stdPrefix ) ThrowArgumentException( m, $"new AutoStandardPrefix '{stdPrefix}' differ from '{m.AutoStandardPrefix}'." );
        }

        void ThrowArgumentException( MeasureUnit m, string explain )
        {
            throw new ArgumentException( $"Already existing unit registration (context '{Name}') for '{m.ToString()}': " + explain );
        }

    }
}

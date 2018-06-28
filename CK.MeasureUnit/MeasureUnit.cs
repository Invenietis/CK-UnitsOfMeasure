using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// The base class for all measure unit also handles the combination of multiples <see cref="MeasureUnit"/>.
    /// </summary>
    public partial class MeasureUnit
    {
        readonly MeasureContext _ctx;
        readonly ExponentMeasureUnit[] _units;
        MeasureUnit _invert;
        FullFactor _normalizationFactor;
        MeasureUnit _normalization;

        /// <summary>
        /// This ctor is used by <see cref="AtomicMeasureUnit"/>: it initializes a
        /// MeasureUnit bound to itself.
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="abbreviation">The abbreviatiuon.</param>
        /// <param name="name">The name.</param>
        /// <param name="isNormalized">True if this measure unit is the normalized one for its dimension.</param>
        private protected MeasureUnit( MeasureContext ctx, string abbreviation, string name, bool isNormalized )
        {
            _ctx = ctx;
            Abbreviation = abbreviation;
            Name = name;
            _units = new[] { (ExponentMeasureUnit)this };
            if( isNormalized )
            {
                _normalizationFactor = FullFactor.Neutral;
                _normalization = this;
            }
        }

        internal MeasureUnit( MeasureContext ctx, (string A, string N) names, ExponentMeasureUnit[] units )
        {
            _ctx = ctx;
            Debug.Assert( !units.Any( u => u.AtomicMeasureUnit == None ) );
            Abbreviation = names.A;
            Name = names.N;
            _units = units;
            if( units.All( u => u.IsNormalized ) )
            {
                _normalizationFactor = FullFactor.Neutral;
                _normalization = this;
            }
        }

        /// <summary>
        /// Gets the context to which this unit of measure belongs.
        /// </summary>
        public MeasureContext Context => _ctx;

        /// <summary>
        /// Gets the abbreviation that identifies this measure.
        /// </summary>
        public string Abbreviation { get; }

        /// <summary>
        /// Gets the long name of this measure.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the one or more <see cref="ExponentMeasureUnit"/> that define this measure.
        /// </summary>
        public IReadOnlyList<ExponentMeasureUnit> MeasureUnits => _units;

        /// <summary>
        /// Gets whether this <see cref="MeasureUnits"/> only contains <see cref="FundamentalMeasureUnit"/>.
        /// </summary>
        public bool IsNormalized => _normalization == this;

        /// <summary>
        /// Gets the factor that must be applied from this measure to its <see cref="Normalization"/>.
        /// </summary>
        public FullFactor NormalizationFactor
        {
            get
            {
                if( _normalization == null )
                {
                    (_normalization, _normalizationFactor) = GetNormalization();
                }
                return _normalizationFactor;
            }
        }

        /// <summary>
        /// Gets the canonical form of this measure.
        /// Its <see cref="IsNormalized"/> property is necessarily true.
        /// </summary>
        public MeasureUnit Normalization
        {
            get
            {
                if( _normalization == null )
                {
                    (_normalization, _normalizationFactor) = GetNormalization();
                }
                return _normalization;
            }
        }


        private protected virtual (MeasureUnit, FullFactor) GetNormalization()
        {
            Combinator measures = new Combinator( null );
            var f = _units.Aggregate( FullFactor.Neutral, ( acc, m ) =>
            {
                measures.Add( m.Normalization.MeasureUnits );
                return acc.Multiply( m.NormalizationFactor );
            } );
            return (measures.GetResult( _ctx ), f);
        }

        /// <summary>
        /// Returns the <see cref="MeasureUnit"/> that results from this one multiplied by another one.
        /// </summary>
        /// <param name="m">Other units to multiply.</param>
        /// <returns>The result of the multiplication.</returns>
        public MeasureUnit Multiply( MeasureUnit m ) => Combinator.Create( _ctx, MeasureUnits.Concat( m.MeasureUnits ) );

        /// <summary>
        /// Returns the <see cref="MeasureUnit"/> that results from this one divided by another one.
        /// </summary>
        /// <param name="m">The divisor.</param>
        /// <returns>The result of the division.</returns>
        public MeasureUnit DivideBy( MeasureUnit m ) => Combinator.Create( _ctx, MeasureUnits.Concat( m.Invert().MeasureUnits ) );

        /// <summary>
        /// Returns this measure of units elevated to a given power.
        /// Note that when <paramref name="exp"/> is 0, <see cref="MeasureUnit.None"/> is returned.
        /// </summary>
        /// <param name="exp">The exponent.</param>
        /// <returns>The resulting normalized units.</returns>
        public MeasureUnit Power( int exp )
        {
            if( exp == 0 ) return None;
            if( exp == 1 ) return this;
            if( exp == -1 ) return Invert();
            Combinator c = new Combinator( null );
            foreach( var m in MeasureUnits )
            {
                if( m.AtomicMeasureUnit != None ) c.Add( m.AtomicMeasureUnit, m.Exponent * exp );
            }
            return c.GetResult( _ctx );
        }

        /// <summary>
        /// Inverts this <see cref="MeasureUnit"/>.
        /// </summary>
        /// <returns>The inverted units.</returns>
        public MeasureUnit Invert()
        {
            if( _invert == null )
            {
                Combinator c = new Combinator( null );
                foreach( var m in MeasureUnits )
                {
                    if( m.AtomicMeasureUnit != None ) c.Add( m.AtomicMeasureUnit, -m.Exponent );
                }
                if( _invert == null )
                {
                    _invert = c.GetResult( _ctx );
                    _invert._invert = this;
                }
            }
            return _invert;
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
        public static AliasMeasureUnit DefineAlias( string abbreviation, string name, FullFactor definitionFactor, MeasureUnit definition )
        {
            return StandardMeasureContext.Default.DefineAlias( abbreviation, name, definitionFactor, definition );
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
        public static FundamentalMeasureUnit DefineFundamental( string abbreviation, string name, MeasureStandardPrefix normalizedPrefix = null )
        {
            return StandardMeasureContext.Default.DefineFundamental( abbreviation, name, normalizedPrefix );
        }

        public static MeasureUnit operator /( MeasureUnit o1, MeasureUnit o2 ) => o1.DivideBy( o2 );
        public static MeasureUnit operator *( MeasureUnit o1, MeasureUnit o2 ) => o1.Multiply( o2 );
        public static MeasureUnit operator ^( MeasureUnit o, int exp ) => o.Power( exp );

        /// <summary>
        /// Returns the abbreviation optionally suffixed with its " (<see cref="Name"/>)".
        /// </summary>
        /// <param name="withName">True to include the Name.</param>
        /// <returns>This measure abbreviation and name.</returns>
        public string ToString( bool withName ) => withName ? $"{Abbreviation} ({Name})" : Abbreviation;

        /// <summary>
        /// Overridden to return the <see cref="Abbreviation"/>.
        /// </summary>
        /// <returns>This unit's abbreviation.</returns>
        public override string ToString() => Abbreviation;

    }
}

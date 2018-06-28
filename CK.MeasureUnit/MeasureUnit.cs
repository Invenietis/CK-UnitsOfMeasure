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

        readonly ExponentMeasureUnit[] _units;
        MeasureUnit _invert;
        FullFactor _normalizationFactor;
        MeasureUnit _normalization;

        /// <summary>
        /// This ctor is used by <see cref="AtomicMeasureUnit"/>: it initializes a
        /// MeasureUnit bound to itself.
        /// </summary>
        /// <param name="abbreviation">The abbreviatiuon.</param>
        /// <param name="name">The name.</param>
        private protected MeasureUnit( string abbreviation, string name, bool isNormalized )
        {
            Abbreviation = abbreviation;
            Name = name;
            _units = new[] { (ExponentMeasureUnit)this };
            if( isNormalized )
            {
                _normalizationFactor = FullFactor.Neutral;
                _normalization = this;
            }
        }

        internal MeasureUnit( (string A, string N) names, ExponentMeasureUnit[] units )
        {
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
            return (measures.GetResult(), f);
        }

        /// <summary>
        /// Returns the <see cref="MeasureUnit"/> that results from this one multiplied by another one.
        /// </summary>
        /// <param name="m">Other units to multiply.</param>
        /// <returns>The result of the multiplication.</returns>
        public MeasureUnit Multiply( MeasureUnit m ) => Combinator.Create( MeasureUnits.Concat( m.MeasureUnits ) );

        /// <summary>
        /// Returns the <see cref="MeasureUnit"/> that results from this one divided by another one.
        /// </summary>
        /// <param name="m">The divisor.</param>
        /// <returns>The result of the division.</returns>
        public MeasureUnit DivideBy( MeasureUnit m ) => Combinator.Create( MeasureUnits.Concat( m.Invert().MeasureUnits ) );

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
            return c.GetResult();
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
                    _invert = c.GetResult();
                    _invert._invert = this;
                }
            }
            return _invert;
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

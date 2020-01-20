using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// Immutable struct that encapsulates a double <see cref="Value"/> and its <see cref="MeasureUnit"/>.
    /// </summary>
    public struct Quantity : IEquatable<Quantity>, IComparable<Quantity>
    {
        /// <summary>
        /// This is the "G15" format used when calling <see cref="Double.ToString(string, IFormatProvider)"/> (with the <see cref="CultureInfo.InvariantCulture"/>)
        /// to obtain a string where small rounding adjustments are erased.
        /// <para>The float formatting has changed with .Net Core 3.0 - see https://devblogs.microsoft.com/dotnet/floating-point-parsing-and-formatting-improvements-in-net-core-3-0/ -
        /// so that the ToString is now "roundtrippable" by default.
        /// The "G15" format is the previous default one that worked perfectly well for our "equality".</para>
        /// </summary>
        public const string ValueRoundedFormat = "G15";

        readonly MeasureUnit _unit;

        /// <summary>
        /// The value.
        /// </summary>
        public readonly double Value;

        /// <summary>
        /// The unit of measure of the <see cref="Value"/>.
        /// Never null.
        /// </summary>
        public MeasureUnit Unit => _unit ?? MeasureUnit.None;

        string _normalized;

        /// <summary>
        /// Initializes a new <see cref="Quantity"/>.
        /// </summary>
        /// <param name="v">Quantity value.</param>
        /// <param name="unit">Unit of measure. Can be null (<see cref="MeasureUnit.None"/> is used).</param>
        public Quantity( double v, MeasureUnit unit )
        {
            Value = v;
            _unit = unit ?? MeasureUnit.None;
            _normalized = null;
        }

        /// <summary>
        /// Multiplies this quantity with another one.
        /// This is always possible: the resulting quantity's unit will hold the combined unit.
        /// </summary>
        /// <param name="q">Quantity to multiply.</param>
        /// <returns>The resulting quantity.</returns>
        public Quantity Multiply( Quantity q ) => new Quantity( Value * q.Value, Unit * q.Unit );


        /// <summary>
        /// Divides this quantity with another one.
        /// This is always possible: the resulting quantity's unit will hold the combined unit.
        /// </summary>
        /// <param name="q">Quantity divisor.</param>
        /// <returns>The resulting quantity.</returns>
        public Quantity DivideBy( Quantity q ) => new Quantity( Value / q.Value, Unit / q.Unit );

        /// <summary>
        /// Inverts this quantity.
        /// The result' value is 1/<see cref="Value"/> and its <see cref="Unit"/> is <see cref="MeasureUnit.Invert"/>.
        /// </summary>
        /// <returns>The inverse quantity.</returns>
        public Quantity Invert() => new Quantity( 1.0 / Value, Unit.Invert() );

        /// <summary>
        /// Elevates this quantity to a given power.
        /// </summary>
        /// <param name="exp">The exponent.</param>
        /// <returns>The resulting quantity.</returns>
        public Quantity Power( int exp ) => new Quantity( Math.Pow( Value, exp ), Unit.Power( exp ) );

        /// <summary>
        /// Checks whether another quantity can be added to this one.
        /// The adqed quantity must be convertible (see <see cref="CanConvertTo"/>) into this <see cref="Unit"/>.
        /// </summary>
        /// <param name="q">The quantity that may be added to this quantity.</param>
        /// <returns>True if <see cref="Add"/> can be called.</returns>
        public bool CanAdd( Quantity q ) => q.CanConvertTo( Unit );

        /// <summary>
        /// Adds a given quantity to this one, returning a quantity with this <see cref="Unit"/>.
        /// The quantity to add must be convertible into this <see cref="Unit"/> (<see cref="CanAdd"/> must be true)
        /// otherwise an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="q">The quantity to add.</param>
        /// <returns>A quantity with this <see cref="Unit"/>.</returns>
        public Quantity Add( Quantity q )
        {
            if( q.Unit == Unit ) return new Quantity( Value + q.Value, Unit );
            var qC = q.ConvertTo( Unit );
            return new Quantity( Value + qC.Value, Unit );
        }

        /// <summary>
        /// Negates this quantity: it is the negated <see cref="Value"/> with the same <see cref="Unit"/>.
        /// </summary>
        /// <returns>The negated quantity.</returns>
        public Quantity Negate() => new Quantity( -Value, Unit );

        /// <summary>
        /// Checks whether this quantity can be converted into a quantity with a different <see cref="Unit"/>.
        /// </summary>
        /// <param name="u">The target unit.</param>
        /// <returns>True if this quantity can be expressed in the target unit, false otherwise.</returns>
        public bool CanConvertTo( MeasureUnit u )
        {
            var o = Unit;
            return o == u
                   || o.Normalization == u.Normalization
                   || o.Normalization == u.Normalization.Invert();
        }

        /// <summary>
        /// Converts this quantity from this <see cref="Unit"/> to another <see cref="MeasureUnit"/>.
        /// Must be called only if <see cref="CanConvertTo(MeasureUnit)"/> returned true otherwise an <see cref="ArgumentException"/>
        /// is thrown.
        /// </summary>
        /// <param name="u">The target unit of measure.</param>
        /// <returns>The quantity exporessed with the target unit.</returns>
        public Quantity ConvertTo( MeasureUnit u )
        {
            if( Unit == u ) return this;
            if( !CanConvertTo( u ) )
            {
                if( u.Context != Unit.Context )
                    throw new ArgumentException( $"Can not convert between units in different contexts ('{Unit}' to '{u}')." );
                throw new ArgumentException( $"Can not convert from '{Unit}' to '{u}'." );
            }
            if( Unit.Normalization == u.Normalization )
            {
                FullFactor ratio = Unit.NormalizationFactor.DivideBy( u.NormalizationFactor );
                return new Quantity( Value * ratio.ToDouble(), u );
            }
            else
            {
                FullFactor ratio = Unit.NormalizationFactor.Multiply( u.NormalizationFactor );
                return new Quantity( 1/(Value * ratio.ToDouble()), u );
            }
        }

#pragma warning disable 1591
        public static Quantity operator /( Quantity o1, Quantity o2 ) => o1.DivideBy( o2 );
        public static Quantity operator *( Quantity o1, Quantity o2 ) => o1.Multiply( o2 );
        public static Quantity operator ^( Quantity o, int exp ) => o.Power( exp );
        public static Quantity operator /( Quantity o, double v ) => new Quantity( o.Value / v, o.Unit );
        public static Quantity operator *( Quantity o, double v ) => new Quantity( o.Value * v, o.Unit );
        public static Quantity operator /( double v, Quantity o ) => new Quantity( v / o.Value, o.Unit.Invert() );
        public static Quantity operator *( double v, Quantity o ) => new Quantity( o.Value * v, o.Unit );
        public static Quantity operator +( Quantity o1, Quantity o2 ) => o1.Add( o2 );
        public static Quantity operator -( Quantity o ) => o.Negate();
        public static Quantity operator -( Quantity o1, Quantity o2 ) => o1.Add( o2.Negate() );
        public static bool operator ==( Quantity o1, Quantity o2 ) => o1.Equals( o2 );
        public static bool operator !=( Quantity o1, Quantity o2 ) => !o1.Equals( o2 );
        public static bool operator >( Quantity o1, Quantity o2 ) => o1.CompareTo( o2 ) > 0;
        public static bool operator <( Quantity o1, Quantity o2 ) => o1.CompareTo( o2 ) < 0;
        public static bool operator >=( Quantity o1, Quantity o2 ) => o1.CompareTo( o2 ) >= 0;
        public static bool operator <=( Quantity o1, Quantity o2 ) => o1.CompareTo( o2 ) <= 0;
#pragma warning restore 1591

        /// <summary>
        /// Get this string representation of this <see cref="Value"/> with this <see cref="Unit"/>.
        /// </summary>
        /// <param name="format">A numeric format string.</param>
        /// <param name="provider">The format provider.</param>
        /// <returns>A readable string.</returns>
        public string ToString( string format, IFormatProvider provider ) => Value.ToString( format, provider )
                                                                              + (Unit != MeasureUnit.None
                                                                                 ? " " + Unit.Abbreviation
                                                                                 : String.Empty);

        /// <summary>
        /// Get this string representation of this <see cref="Value"/> with this <see cref="Unit"/>,
        /// using <see cref="ValueRoundedFormat"/> and <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="format">A numeric format string.</param>
        /// <param name="provider">The format provider.</param>
        /// <returns>A readable string.</returns>
        public string ToRoundedString() => ToString( "G15", CultureInfo.InvariantCulture );

        /// <summary>
        /// Get this string representation of this <see cref="Value"/> with this <see cref="Unit"/>.
        /// </summary>
        /// <param name="provider">The format provider.</param>
        /// <returns>A readable string.</returns>
        public string ToString( IFormatProvider provider ) => Value.ToString( provider )
                                                              + (Unit != MeasureUnit.None
                                                                 ? " " + Unit.Abbreviation
                                                                 : String.Empty);

        /// <summary>
        /// Overridden to return this string representation of this <see cref="Value"/> with this <see cref="Unit"/>.
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => Value.ToString()
                                                + (Unit != MeasureUnit.None
                                                    ? " " + Unit.Abbreviation
                                                    : String.Empty);

        /// <summary>
        /// Returns the <see cref="ToRoundedString()"/> representation of this quantity in
        /// this <see cref="Unit"/>'s <see cref="MeasureUnit.Normalization"/>.
        /// </summary>
        /// <returns>A readable string.</returns>
        public string ToNormalizedString()
        {
            if( _normalized == null )
            {
                _normalized = Unit == null ? "0" : ConvertTo( Unit.Normalization ).ToRoundedString();
            }
            return _normalized;
        }

        /// <summary>
        /// Overridden to support Unit aware equality. See <see cref="Equals(Quantity)"/>.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if this quantity is the same as the one, false otherwise.</returns>
        public override bool Equals( object obj ) => obj is Quantity q && Equals( q );

        /// <summary>
        /// Overridden to support Unit aware equality.
        /// </summary>
        /// <returns>The hash code to use for this quantity.</returns>
        public override int GetHashCode() => Unit.Normalization.GetHashCode() ^ ToNormalizedString().GetHashCode();

        /// <summary>
        /// Checks if this quantity is equal to another one: its <see cref="ToNormalizedString"/>
        /// is the same as the other quantity (and they belong to the same <see cref="MeasureContext"/>).
        /// </summary>
        /// <param name="other">The quantity that may be equal to this.</param>
        /// <returns>True if this quantity is the same as the other one, false otherwise.</returns>
        public bool Equals( Quantity other ) => Unit.Context == other.Unit.Context
                                                && ToNormalizedString() == other.ToNormalizedString();

        /// <summary>
        /// Compares this quantity to another one.
        /// </summary>
        /// <param name="other">The other quantity to compare.</param>
        /// <returns></returns>
        public int CompareTo( Quantity other )
        {
            var tU = Unit;
            var oU = other.Unit;
            // Do the 2 units belong to the same (possibly null) context?
            if( tU.Context == oU.Context )
            {
                // First chexk our equality: we must do this first to ensure coherency.
                if( ToNormalizedString() == other.ToNormalizedString() )
                {
                    return 0;
                }
                // Same unit, we compare the Values.
                if( tU == oU )
                {
                    return Value.CompareTo( other.Value );
                }
                // Same normalized units, we convert this Value to the other unit before comparison.
                if( tU.Normalization == oU.Normalization )
                {
                    FullFactor ratio = tU.NormalizationFactor.DivideBy( oU.NormalizationFactor );
                    return (Value * ratio.ToDouble()).CompareTo( other.Value );
                }
                // Inverted normalized units, we convert this Value to the other unit before comparison.
                if( tU.Normalization == oU.Normalization.Invert() )
                {
                    FullFactor ratio = tU.NormalizationFactor.Multiply( oU.NormalizationFactor );
                    return (1/(Value * ratio.ToDouble())).CompareTo( other.Value );
                }
                // No possible conversion. How to compare kilograms and milliSievert?
                // Using their abbreviation (here kilogram will be "smaller" than milliSievert)
                return tU.Abbreviation.CompareTo( oU.Abbreviation );
            }
            // Not in the same context.
            if( tU == MeasureUnit.None ) return -1;
            if( oU == MeasureUnit.None ) return 1;
            return tU.Context.Name.CompareTo( oU.Context.Name );
        }
    }
}

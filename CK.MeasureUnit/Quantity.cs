using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    public struct Quantity
    {
        public readonly double Value;
        public readonly MeasureUnit Unit;

        public Quantity( double v, MeasureUnit u )
        {
            Value = v;
            Unit = u;
        }

        public Quantity Multiply( Quantity q ) => new Quantity( Value * q.Value, Unit * q.Unit );

        public Quantity DivideBy( Quantity q ) => new Quantity( Value / q.Value, Unit / q.Unit );

        public Quantity Invert() => new Quantity( 1.0 / Value, Unit.Invert() );

        public Quantity Power( int exp ) => new Quantity( Math.Pow( Value, exp ), Unit.Power( exp ) );

        public Quantity ConvertTo( MeasureUnit u )
        {
            if( !CanConvertTo( u ) )
            {
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

        public bool CanConvertTo( MeasureUnit u ) => Unit.Normalization == u.Normalization || Unit.Normalization == u.Normalization.Invert();

        public static Quantity operator /( Quantity o1, Quantity o2 ) => o1.DivideBy( o2 );
        public static Quantity operator *( Quantity o1, Quantity o2 ) => o1.Multiply( o2 );
        public static Quantity operator ^( Quantity o, int exp ) => o.Power( exp );

        public string ToString( IFormatProvider formatProvider ) => Value.ToString( formatProvider ) + " " + Unit.ToString();

        public override string ToString() => $"{Value} {Unit}";

    }
}

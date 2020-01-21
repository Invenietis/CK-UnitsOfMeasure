using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.UnitsOfMeasure.Tests
{
    [TestFixture]
    public class QuantityTests
    {
        [Test]
        public void simple_operations()
        {
            var c = new StandardMeasureContext( "Empty" );
            var metre = c.Metre;
            var second = c.Second;
            var kilometre = MeasureStandardPrefix.Kilo[metre];
            var minute = c.DefineAlias( "min", "Minute", 60, second );
            var hour = c.DefineAlias( "h", "Hour", 60, minute );
            var speed = kilometre / hour;

            var myDistance = 3.WithUnit( kilometre );
            var mySpeed = 6.WithUnit( speed );
            var myTime = myDistance / mySpeed;

            myTime.ToString( CultureInfo.InvariantCulture ).Should().Be( "0.5 h" );

            myTime.CanConvertTo( minute ).Should().BeTrue();
            myTime.ConvertTo( minute ).ToString().Should().Be( "30 min" );

            myTime.CanConvertTo( second ).Should().BeTrue();
            myTime.ConvertTo( second ).ToString().Should().Be( "1800 s" );
        }


        [Test]
        public void poetic_units()
        {
            var metre = MeasureUnit.Metre;
            var decimetre = MeasureStandardPrefix.Deci[metre];
            var centimetre = MeasureStandardPrefix.Centi[metre];
            var kilometre = MeasureStandardPrefix.Kilo[metre];
            var hundredKilometre = MeasureStandardPrefix.Hecto[kilometre];
            var litre = decimetre ^ 3;

            var inch = MeasureUnit.DefineAlias( "in", "Inch", 2.54, centimetre );
            var gallon = MeasureUnit.DefineAlias( "gal", "US Gallon", 231, inch ^ 3 );

            var mile = MeasureUnit.DefineAlias( "mile", "Mile", 1.609344, kilometre );

            var milesPerGalon = mile / gallon;
            var litrePerHundredKilometer = litre / hundredKilometre;

            milesPerGalon.Normalization.Abbreviation.Should().Be( "m-2" );
            litrePerHundredKilometer.Normalization.Abbreviation.Should().Be( "m2" );

            var oneMilesPerGallon = 1.WithUnit( milesPerGalon );

            oneMilesPerGallon.CanConvertTo( litrePerHundredKilometer ).Should().BeTrue();
            var result = oneMilesPerGallon.ConvertTo( litrePerHundredKilometer );

            result.Value.Should().BeApproximately( 235.215, 1e-3 );
        }

        [Test]
        public void Quantity_operators_override()
        {
            var d1 = 1.WithUnit( MeasureUnit.Metre );
            var d2 = 2.WithUnit( MeasureUnit.Metre );
            var d3 = d1 + d2;
            d3.Value.Should().Be( 3.0 );

            var d6 = d3 * 2;
            d6.Value.Should().Be( 6.0 );

            var s36 = d6 ^ 2;
            s36.Value.Should().Be( 36.0 );
            s36.Unit.Should().Be( MeasureUnit.Metre * MeasureUnit.Metre );

            var sM36 = -s36;
            sM36.Value.Should().Be( -36.0 );
            sM36.Unit.Should().Be( MeasureUnit.Metre * MeasureUnit.Metre );

            (s36 - sM36).Value.Should().Be( 72.0 );
            (s36 + sM36).Value.Should().Be( 0.0 );

            var s9 = d3 * d3;
            s9.Value.Should().Be( 9.0 );
            s9.Unit.Should().Be( MeasureUnit.Metre * MeasureUnit.Metre );

            var r4 = s36 / s9;
            r4.Value.Should().Be( 4 );
            r4.Unit.Should().Be( MeasureUnit.None );

            var s144 = r4 * s36;
            s144.Value.Should().Be( 144.0 );
            s144.Unit.Should().Be( MeasureUnit.Metre * MeasureUnit.Metre );

            var v27 = d3 ^ 3;
            v27.Value.Should().Be( 27.0 );
            v27.Unit.Should().Be( MeasureUnit.Metre ^ 3 );

            (v27 / s9).Should().Be( d3 );
            ((v27 / s9) == d3).Should().BeTrue();
            ((v27 / s9) != d3).Should().BeFalse();

            (v27 / s9).GetHashCode().Should().Be( d3.GetHashCode() );

        }

        [Test]
        public void Quantity_with_alias_and_prefixed_units_with_metre()
        {
            var metre = MeasureUnit.Metre;
            var decametre = MeasureStandardPrefix.Deca[metre];
            var decimetre = MeasureStandardPrefix.Deci[metre];

            var dm1 = 1.WithUnit( decimetre );
            var dam1 = 1.WithUnit( decametre );
            var dm101 = dm1 + dam1;
            var dam1Dot01 = dam1 + dm1;

            dm101.ToString( CultureInfo.InvariantCulture ).Should().Be( "101 dm" );
            dam1Dot01.ToString( CultureInfo.InvariantCulture ).Should().Be( "1.01 dam" );

            dm101.Equals( dam1Dot01 ).Should().BeTrue();
            dam1Dot01.Equals( dm101 ).Should().BeTrue();
            dm101.ConvertTo( metre ).ToRoundedString().Should().Be( "10.1 m" );
            dam1Dot01.ConvertTo( metre ).ToRoundedString().Should().Be( "10.1 m" );

            dm101.GetHashCode().Should().Be( dam1Dot01.GetHashCode() );
        }

        [Test]
        public void Quantity_with_alias_and_prefixed_units_with_gram()
        {
            var gram = MeasureUnit.Gram;
            var decagram = MeasureStandardPrefix.Deca[gram];
            var decigram = MeasureStandardPrefix.Deci[gram];

            var dg1 = 1.WithUnit( decigram );
            var dag1 = 1.WithUnit( decagram );
            var dg101 = dg1 + dag1;
            var dag1Dot01 = dag1 + dg1;

            dg101.ToString( CultureInfo.InvariantCulture ).Should().Be( "101 dg" );
            dag1Dot01.ToString( CultureInfo.InvariantCulture ).Should().Be( "1.01 dag" );

            dg101.Equals( dag1Dot01 ).Should().BeTrue();
            dag1Dot01.Equals( dg101 ).Should().BeTrue();
            dg101.ConvertTo( gram ).ToRoundedString().Should().Be( "10.1 g" );
            dag1Dot01.ConvertTo( gram ).ToRoundedString().Should().Be( "10.1 g" );

            dg101.GetHashCode().Should().Be( dag1Dot01.GetHashCode() );
            dg101.ToNormalizedString().Should().Be( "0.0101 kg" );
        }

        [Test]
        public void Quantity_kindly_handle_the_default_quantity_with_null_Unit()
        {
            var qDef = new Quantity();

            qDef.ToNormalizedString().Should().Be( "0" );
            qDef.CanConvertTo( MeasureUnit.None ).Should().BeTrue();

            var kilo = 1.WithUnit( MeasureUnit.Kilogram );
            qDef.CanConvertTo( kilo.Unit ).Should().BeFalse();
            qDef.CanAdd( kilo ).Should().BeFalse();
            kilo.CanAdd( qDef ).Should().BeFalse();

            var zeroKilo = qDef.Multiply( kilo );
            zeroKilo.ToNormalizedString().Should().Be( "0 kg" );

            (qDef * kilo).ToNormalizedString().Should().Be( "0 kg" );
            (kilo * qDef).ToNormalizedString().Should().Be( "0 kg" );
            (kilo.Multiply( qDef )).ToNormalizedString().Should().Be( "0 kg" );
            (qDef / kilo).ToNormalizedString().Should().Be( "0 kg-1" );

            var qDef2 = qDef.ConvertTo( MeasureUnit.None );
            qDef2.ToNormalizedString().Should().Be( "0" );
        }

        [Test]
        public void Quantity_comparison()
        {
            var r0 = new Quantity();
            var rM1 = -1.WithUnit( MeasureUnit.None );
            var r2 = 2.WithUnit( MeasureUnit.None );
            var qG1 = 1.WithUnit( MeasureUnit.Gram );
            var qG2 = 2.WithUnit( MeasureUnit.Gram );
            var qM1 = 1.WithUnit( MeasureUnit.Metre );
            var qM2 = 2.WithUnit( MeasureUnit.Metre );

            var cA = new StandardMeasureContext( "A" );
            var cB = new StandardMeasureContext( "B" );

            var q2CA = 3.WithUnit( cA.Gram );
            var q1CB = -1.WithUnit( cB.Gram );

            var l = new[] { q2CA, q1CB, qM2, qM1, qG1, r0, qG2, r2, rM1 };
            Array.Sort( l );
            String.Join( ", ", l.Select( e => $"{e} ({(e.Unit.Context == null ? "*" : e.Unit.Context.Name)})" ) )
                .Should().Be( "-1 (*), 0 (*), 2 (*), 1 g (), 2 g (), 1 m (), 2 m (), 3 g (A), -1 g (B)" );
        }

        [Test]
        public void informational_quantities()
        {
            var kiloByte = MeasureStandardPrefix.Kilo[MeasureUnit.Byte];
            var kibiByte = MeasureStandardPrefix.Kibi[MeasureUnit.Byte];

            var kb80 = 80.WithUnit( kiloByte );
            kb80.CanConvertTo( MeasureUnit.Bit ).Should().BeTrue();
            kb80.ConvertTo( MeasureUnit.Bit ).ToString().Should().Be( "640000 b" );

            var kib80 = kb80.ConvertTo( kibiByte );
            kib80.ToString( CultureInfo.InvariantCulture ).Should().Be( "78.125 KiB" );

        }

        [Test]
        public void dimensionless_quantity_like_percent_or_permille_works()
        {
            var percent = MeasureUnit.DefineAlias( "%", "Percent", new ExpFactor( 0, -2 ), MeasureUnit.None );
            var permille = MeasureUnit.DefineAlias( "‰", "Permille", new ExpFactor( 0, -3 ), MeasureUnit.None );
            var pertenthousand = MeasureUnit.DefineAlias( "‱", "Pertenthousand", new ExpFactor( 0, -4 ), MeasureUnit.None );

            var pc10 = 10.WithUnit( percent );
            pc10.ToString().Should().Be( "10 %" );

            var pm20 = 20.WithUnit( permille );
            pm20.ToString().Should().Be( "20 ‰" );

            var pt30 = 30.WithUnit( pertenthousand );
            pt30.ToString().Should().Be( "30 ‱" );

            (pc10 * pm20 * pt30).ToString().Should().Be("6000 10^-9");

            (pc10 + pm20 + pt30).ToString( CultureInfo.InvariantCulture ).Should().Be( "12.3 %" );
            (pt30  + pc10 + pm20).ToString( CultureInfo.InvariantCulture ).Should().Be( "1230 ‱" );

            var km = MeasureStandardPrefix.Kilo[MeasureUnit.Metre];
            var km100 = 100.WithUnit( MeasureStandardPrefix.Kilo[MeasureUnit.Metre] );
            var pc10OfKm100 = pc10 * km100;
            pc10OfKm100.ToString().Should().Be( "1000 10^-2.km" );
            pc10OfKm100.ConvertTo( km ).ToString().Should().Be( "10 km" );
        }

        /// <summary>
        /// Big thanks to Dave Hary (https://www.codeproject.com/script/Membership/View.aspx?mid=284040): dimensionless "units"
        /// are convertible into each other.
        /// </summary>
        [Test]
        public void percent_or_other_dimensionless_units_are_convertible_into_each_other()
        {
            var percent = MeasureUnit.DefineAlias( "%", "Percent", new ExpFactor( 0, -2 ), MeasureUnit.None );
            var pc10 = 10.WithUnit( percent );
            pc10.ToString().Should().Be( "10 %" );
            pc10.CanConvertTo( MeasureUnit.None ).Should().Be( true, $"{percent.Name} should be convertible to {MeasureUnit.None.Name}" );

            var noDimension = pc10.ConvertTo( MeasureUnit.None );
            noDimension.Unit.Should().Be( MeasureUnit.None );
            noDimension.Value.Should().Be( 0.01 * pc10.Value, $"is {pc10.ToString()}" );

            // Now, express the ratio in permille.
            var permille = MeasureUnit.DefineAlias( "‰", "Permille", new ExpFactor( 0, -3 ), MeasureUnit.None );
            noDimension.CanConvertTo( permille ).Should().BeTrue();
            var inPerMille = noDimension.ConvertTo( permille );
            inPerMille.Unit.Should().Be( permille );
            inPerMille.Value.Should().Be( 10 * pc10.Value, $"is {pc10.ToString()}" );
        }

        /// <summary>
        /// Big thanks to Dave Hary (https://www.codeproject.com/script/Membership/View.aspx?mid=284040) for having
        /// spotted this!
        /// </summary>
        [Test]
        public void combining_same_dimension_leads_to_dimensionless_quantities()
        {
            var ratio = (1.WithUnit( MeasureUnit.Kilogram ) / 1.WithUnit( MeasureUnit.Gram ));

            ratio.Unit.Normalization.Should().Be( MeasureUnit.None, $"{MeasureUnit.Kilogram } / {MeasureUnit.Gram} is dimensionless" );

            ratio.CanConvertTo( MeasureUnit.None ).Should().Be( true, $"{MeasureUnit.Kilogram } / {MeasureUnit.Gram} is dimensionless" );

            var pureRatio = ratio.ConvertTo( MeasureUnit.None );

            pureRatio.Value.Should().Be( 1000 );
        }

    }
}

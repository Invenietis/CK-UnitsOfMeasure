using FluentAssertions;
using System;
using NUnit.Framework;

namespace CK.UnitsOfMeasure.Tests
{

    [TestFixture]
    public class NormalizationTests
    {
        [Test]
        public void basic_normalization_with_prefix()
        {
            var metre = MeasureUnit.Metre;
            var squaredMeter = metre ^ 2;
            squaredMeter.Normalization.Should().Be( squaredMeter );

            var decimeter = MeasureStandardPrefix.Deci[metre];
            var squaredDecimeter = decimeter ^ 2;
            squaredDecimeter.Normalization.Should().Be( squaredMeter );
            squaredDecimeter.NormalizationFactor.Should().Be( new FullFactor( new ExpFactor( 0, -2 ) ), "1 dm2 = 10-2 m2" );

            var centimeter = MeasureStandardPrefix.Centi[metre];
            var squaredCentimeter = centimeter ^ 2;
            squaredCentimeter.Normalization.Should().Be( squaredMeter );
            squaredCentimeter.NormalizationFactor.Should().Be( new FullFactor( new ExpFactor( 0, -4 ) ), "1 cm2 = 10-4 m2" );
        }

        [Test]
        public void equivalent_combined_units()
        {
            var metre = MeasureUnit.Metre;
            var second = MeasureUnit.Second;

            var acceleration = metre / (second ^ 2);
            acceleration.IsNormalized.Should().BeTrue();

            var micrometre = MeasureStandardPrefix.Micro[metre];
            var millisecond = MeasureStandardPrefix.Milli[second];

            var acceleration2 = micrometre / (millisecond ^ 2);
            acceleration2.IsNormalized.Should().BeFalse();
            acceleration2.Normalization.Should().BeSameAs( acceleration );
            acceleration2.NormalizationFactor.Should().Be( FullFactor.Neutral );
        }

        [Test]
        public void combined_units_with_factor()
        {
            var metre = MeasureUnit.Metre;
            var second = MeasureUnit.Second;
            var speed = metre / second;
            speed.IsNormalized.Should().BeTrue();

            var kilometre = MeasureStandardPrefix.Kilo[metre];
            var hour = MeasureUnit.DefineAlias( "h", "Hour", 60 * 60, second );
            var speed2 = kilometre / hour;
            speed2.IsNormalized.Should().BeFalse();

            speed2.Normalization.Should().BeSameAs( speed );
            speed2.NormalizationFactor.ToDouble().Should().BeApproximately( 0.2777777778, 1e-10, "1 m/s = 0.277778 km/h" );
        }


        [Test]
        public void the_kilogram_is_now_the_normalized_form()
        {
            var metre = MeasureUnit.Metre;
            var second = MeasureUnit.Second;
            var kilogram = MeasureUnit.Kilogram;

            var newton = kilogram * metre * (second ^ -2);
            newton.Abbreviation.Should().Be( "kg.m.s-2" );

            // With FundamentalMeasureUnit as the only normalized form, we previously had:
            //newton.Normalization.Abbreviation.Should().Be( "g.m.s-2" );
            //newton.NormalizationFactor.Should().Be( new FullFactor( new ExpFactor( 0, 3 ) ) );

            // A PrefixedMeasureUnit can be the normalized form:
            newton.Normalization.Abbreviation.Should().Be( "kg.m.s-2" );
            newton.NormalizationFactor.Should().Be( FullFactor.Neutral );
        }

        [Test]
        public void dimensionless_units()
        {
            // See https://en.wikipedia.org/wiki/Per_mille
            var percent = MeasureUnit.DefineAlias( "%", "Percent", new ExpFactor( 0, -2 ), MeasureUnit.None );
            var permille = MeasureUnit.DefineAlias( "‰", "Permille", new ExpFactor( 0, -3 ), MeasureUnit.None );
            var pertenthousand = MeasureUnit.DefineAlias( "‱", "Pertenthousand", new ExpFactor( 0, -4 ), MeasureUnit.None );

            percent.ToString().Should().Be( "%" );
            percent.Normalization.Should().BeSameAs( MeasureUnit.None );
            percent.NormalizationFactor.Should().Be( new FullFactor( new ExpFactor( 0, -2 ) ) );

            var perht = percent * permille;
            perht.ToString().Should().Be( "10^-5" );
            perht.Normalization.Should().BeSameAs( MeasureUnit.None );
            perht.NormalizationFactor.Should().Be( new FullFactor( new ExpFactor( 0, -5 ) ) );
        }
    }
}

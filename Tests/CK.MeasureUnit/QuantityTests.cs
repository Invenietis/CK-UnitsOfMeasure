using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class QuantityTests
    {
        [Test]
        public void simple_operations()
        {
            var metre = MeasureUnit.Metre;
            var second = MeasureUnit.Second;
            var kilometre = MeasureStandardPrefix.Kilo[metre];
            var minute = MeasureUnit.DefineAlias( "min", "Minute", 60, second );
            var hour = MeasureUnit.DefineAlias( "h", "Hour", 60, minute );
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
    }
}

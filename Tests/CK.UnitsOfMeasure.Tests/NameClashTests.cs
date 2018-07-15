using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using static CK.UnitsOfMeasure.MeasureUnit;

namespace CK.UnitsOfMeasure.Tests
{
    [TestFixture]
    public class NameClashTests
    {
        [Test]
        public void new_units_clash_name_detection()
        {
            var c = new StandardMeasureContext( "Empty" );
            var sievert = c.DefineAlias( "Sv", "Sievert", FullFactor.Neutral, (c.Metre ^ 2) * (c.Second ^ 2) );
            MeasureStandardPrefix.All.Select( p => p.Abbreviation + "Sv" )
                                     .Where( a => c.IsValidNewAbbreviation( a ) )
                                     .Should().BeEmpty();

            var x = c.DefineAlias( "xSv", "Bad name anyway", FullFactor.Neutral, c.Ampere );
            x.ToString().Should().Be( "xSv" );

            c.Invoking( sut => sut.DefineAlias( "", "no way", FullFactor.Neutral, c.Metre ) )
                .Should().Throw<ArgumentException>();

            c.Invoking( sut => sut.DefineAlias( "p2", "no digit allowed", FullFactor.Neutral, c.Metre ) )
                .Should().Throw<ArgumentException>();

            c.Invoking( sut => sut.DefineAlias( "p2p", "no digit allowed", FullFactor.Neutral, c.Metre ) )
                .Should().Throw<ArgumentException>();

            c.Invoking( sut => sut.DefineAlias( "damol", "no way", FullFactor.Neutral, c.Metre ) )
                .Should().Throw<ArgumentException>();
        }

        [Test]
        public void when_minute_is_defined_inch_can_no_more_exist()
        {
            var c = new StandardMeasureContext( "Empty" );
            var minute = c.DefineAlias( "min", "Minute", new FullFactor( 60 ), c.Second );
            c.IsValidNewAbbreviation( "in" ).Should().BeFalse();
        }
    }
}


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
            var sievert = c.DefineAlias( "Sv", "Sievert", FullFactor.Neutral, (c.Metre ^ 2) * (c.Second ^ 2), AutoStandardPrefix.Metric );
            MeasureStandardPrefix.MetricPrefixes.Select( p => p.Abbreviation + "Sv" )
                                     .Where( a => c.CheckValidNewAbbreviation( a, AutoStandardPrefix.None ) != MeasureContext.NewAbbreviationConflict.MatchPotentialAutoStandardPrefixedUnit )
                                     .Should().BeEmpty();

            MeasureStandardPrefix.BinaryPrefixes.Select( p => p.Abbreviation + "Sv" )
                                     .Where( a => c.CheckValidNewAbbreviation( a, AutoStandardPrefix.None ) == MeasureContext.NewAbbreviationConflict.MatchPotentialAutoStandardPrefixedUnit )
                                     .Should().BeEmpty();

            var x = c.DefineAlias( "xSv", "Bad name but okay...", FullFactor.Neutral, c.Ampere );
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
        public void minute_and_inch_can_coexist_unless_inch_supports_metric_prefixes()
        {
            var cM = new StandardMeasureContext( "WithMinute" );
            var minute = cM.DefineAlias( "min", "Minute", new FullFactor( 60 ), cM.Second );
            cM.CheckValidNewAbbreviation( "in", AutoStandardPrefix.None ).Should().Be( MeasureContext.NewAbbreviationConflict.None );
            cM.CheckValidNewAbbreviation( "in", AutoStandardPrefix.Binary ).Should().Be( MeasureContext.NewAbbreviationConflict.None );
            cM.CheckValidNewAbbreviation( "in", AutoStandardPrefix.Metric ).Should().Be( MeasureContext.NewAbbreviationConflict.AmbiguousAutoStandardPrefix );

            var cI = new StandardMeasureContext( "WithInchMetric" );
            var inch = cI.DefineAlias( "in",
                                       "Inch",
                                       2.54,
                                       MeasureStandardPrefix.Centi[cI.Metre],
                                       AutoStandardPrefix.Metric );
            cI.CheckValidNewAbbreviation( "min", AutoStandardPrefix.None ).Should().Be( MeasureContext.NewAbbreviationConflict.MatchPotentialAutoStandardPrefixedUnit );
        }

        [Test]
        public void applying_unsupported_prefixes_to_a_unit_uses_the_adjustment_factor()
        {
            var c = new MeasureContext( "Empty" );
            var percent = c.DefineAlias( "%", "Percent", new ExpFactor(0,-2), MeasureUnit.None );
            var milliPercent = MeasureStandardPrefix.Milli[percent];
            milliPercent.ToString().Should().Be( "(10^-3)%" );
        }

    }
}


using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.UnitsOfMeasure.Tests
{
    [TestFixture]
    public class MeasureContextTests
    {
        [Test]
        public void measures_from_different_contexts_must_not_interfere()
        {
            var kilogram = MeasureUnit.Kilogram;
            kilogram.Should().BeSameAs( StandardMeasureContext.Default.Kilogram );

            var another = new StandardMeasureContext( "Another" );
            var anotherKilogram = another.Kilogram;
            anotherKilogram.Should().NotBeSameAs( kilogram );

            another.Invoking( c => c.DefineAlias( "derived", "Derived", 2, kilogram ) )
                .Should().Throw<Exception>().Where( ex => ex.Message.Contains( "Context mismatch" ) );

            StandardMeasureContext.Default.Invoking( c => c.DefineAlias( "derived", "Derived", 2, anotherKilogram ) )
                .Should().Throw<Exception>().Where( ex => ex.Message.Contains( "Context mismatch" ) );


            Action a = () => Console.WriteLine( kilogram / anotherKilogram );
            a.Should().Throw<Exception>().Where( ex => ex.Message.Contains( "Context mismatch" ) );
        }

        [Test]
        public void bad_unit_redefinition_throws_explicit_argument_exceptions()
        {
            StandardMeasureContext.Default.Invoking( x => x.DefineAlias( "&", "", 2.0, MeasureUnit.Metre ) )
                .Should().Throw<ArgumentException>().WithMessage( "*InvalidCharacters*" );

            StandardMeasureContext.Default.Invoking( x => x.DefineAlias( "kK", "ClashWithKiloKelvin", 2.0, MeasureUnit.Metre ) )
                .Should().Throw<ArgumentException>().WithMessage( "*MatchPotentialAutoStandardPrefixedUnit*" );

            StandardMeasureContext.Default.Invoking( x => x.DefineAlias( "d", "TheCentiClashWithCandela", 2.0, MeasureUnit.Metre, AutoStandardPrefix.Metric ) )
                .Should().Throw<ArgumentException>().WithMessage( "*AmbiguousAutoStandardPrefix*" );

            StandardMeasureContext.Default.Invoking( x => x.DefineAlias( "kg", "nimp", 2.0, MeasureUnit.Metre ) )
                .Should().Throw<ArgumentException>().WithMessage( "*new name 'nimp' differ from 'Kilogram'." );

            var c = new StandardMeasureContext( "Test" );
            var candela = c.Candela;
            c.Invoking( x => x.DefineFundamental( candela.Abbreviation, candela.Name, AutoStandardPrefix.Metric ) ).Should().NotThrow();
            c.Invoking( x => x.DefineFundamental( candela.Abbreviation, candela.Name, AutoStandardPrefix.Binary ) )
                .Should().Throw<ArgumentException>().WithMessage( "*new AutoStandardPrefix 'Binary' differ from 'Metric'." );

            var kilometre = MeasureStandardPrefix.Kilo[c.Metre];
            c.Invoking( x => x.DefineAlias( kilometre.Abbreviation, kilometre.Name, new ExpFactor( 0, 3 ), c.Metre ) )
                .Should().Throw<ArgumentException>().WithMessage( "*new type 'AliasMeasureUnit' differ from 'PrefixedMeasureUnit'." );

            c.DefineAlias( "ktg", "Thing", 2.0, c.Unit );
            c.Invoking( x => x.DefineAlias( "ktg", "Thing", 2.0, c.Unit ) ).Should().NotThrow();
            c.Invoking( x => x.DefineAlias( "ktg", "NotThing", 2.0, c.Unit ) )
                .Should().Throw<ArgumentException>().WithMessage( "*new name 'NotThing' differ from 'Thing'." );

            // Use ? at the comma position to handle comma or dot decimal separator.
            c.Invoking( x => x.DefineAlias( "ktg", "Thing", 2.5, c.Unit ) )
                .Should().Throw<ArgumentException>().WithMessage( "*new normalization factor '2?5' should be '2'." );

            var xtg = c.DefineAlias( "XTG", "2.5xThing", 2.5, c.Unit );
            c.DefineAlias( "XXTG", "2xXTG=5*Thing", 2.0, xtg );

            c.Invoking( x => x.DefineAlias( "XXTG", "2xXTG=5*Thing", 3.0, c.Metre ) )
                .Should().Throw<ArgumentException>().WithMessage( "*new definition unit 'm' is not compatible with 'XTG'." );

            c.Invoking( x => x.DefineAlias( "XXTG", "2xXTG=5*Thing", 2.0, c.Unit ) )
                .Should().Throw<ArgumentException>().WithMessage( "*new normalization factor '2' should be '5'." );

            c.Invoking( x => x.DefineAlias( "XXTG", "2xXTG=5*Thing", 3.0, xtg ) )
                .Should().Throw<ArgumentException>().WithMessage( "*new normalization factor '3' should be '2'." );

        }
    }
}

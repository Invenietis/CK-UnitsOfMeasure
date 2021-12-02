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
    public class MarshallerTests
    {
        [Test]
        public void marshalling_measure_with_TryParse_requires_the_Fundamental_and_Alias_units_to_be_defined_on_the_receiver()
        {
            var cA = new MeasureContext( "A" );
            // This one is interesting since it is a fundamental measure but is "official" form is actually a PrefixedMeasureUnit.
            cA.DefineFundamental( "g", "Gram", AutoStandardPrefix.Metric, MeasureStandardPrefix.Kilo );
            var money = cA.DefineFundamental( "¤", "Money" );
            cA.DefineAlias( "$", "Dollar", new FullFactor( 37.12 ), money, AutoStandardPrefix.Metric );

            // Unit price in $ per decagram.
            cA.TryParse( "$.dag-1", out var unitPrice ).Should().BeTrue();
            // The Normalized unit:
            unitPrice.Normalization.ToString().Should().Be( "¤.kg-1" );
            unitPrice.NormalizationFactor.ToString( CultureInfo.InvariantCulture ).Should().Be( "37.12*10^2" );

            var marshalled = unitPrice.ToString();

            var cB = new MeasureContext( "B" );
            // B is not ready to receive the "marshalled" unit.
            cB.TryParse( marshalled, out var _ ).Should().BeFalse();

            // Defining the fundamental Money is not enough.
            cB.DefineFundamental( "¤", "Money" );
            cB.TryParse( marshalled, out var _ ).Should().BeFalse();

            // This is still not enough.
            cB.DefineFundamental( "g", "Gram", AutoStandardPrefix.Metric, MeasureStandardPrefix.Kilo );
            cB.TryParse( marshalled, out var _ ).Should().BeFalse();

            // Defining the missing alias unit on B:
            cB.DefineAlias( "$", "Dollar", new FullFactor( 37.12 ), cB["¤"], AutoStandardPrefix.Metric );
            // Now, it's okay.
            cB.TryParse( marshalled, out var onB ).Should().BeTrue();

            onB.ToString().Should().Be( marshalled );
        }

        [Test]
        public void Extracting_anchors_for_a_MeasureUnit()
        {
            var cA = new MeasureContext( "A" );
            var gram = cA.DefineFundamental( "g", "Gram", AutoStandardPrefix.Metric, MeasureStandardPrefix.Kilo );
            var money = cA.DefineFundamental( "¤", "Money" );
            var dollar = cA.DefineAlias( "$", "Dollar", new FullFactor( 37.12 ), money, AutoStandardPrefix.Metric );

            var unitPrice = dollar / MeasureStandardPrefix.Deca.On( gram );

            var anchors = new List<MeasureUnit>();
            unitPrice.ExtractAnchors( anchors.Add, anchors.Add );

            anchors.Should().HaveCount( 3 );
            // Money comes first since we need money to be able to define dollar.
            // Gram comes last because in the root unitPrice, its -1 exponent makes it the last one.
            anchors.Should().Equal( money, dollar, gram );
        }

        [Test]
        public void defining_an_easy_to_parse_language_to_interpret_the_anchors_and_replay_them_on_a_context()
        {
            var cA = new MeasureContext( "A" );
            var gram = cA.DefineFundamental( "g", "Gram", AutoStandardPrefix.Metric, MeasureStandardPrefix.Kilo );
            var money = cA.DefineFundamental( "¤", "Money" );
            var dollar = cA.DefineAlias( "$", "Dollar", new FullFactor( 37.12 ), money, AutoStandardPrefix.Metric );

            var unitPrice = dollar / MeasureStandardPrefix.Deca.On( gram );

            // The idea here is simple: we must capture everything needed to call the 2 factory methods 
            // and MeasureContext.Alias( ... ) and MeasureContext.CreateFundamental( ... ).
            // We don't need a discriminator between the 2: we can use the cardinality of the "parameters".
            var anchors = new List<string>();
            unitPrice.ExtractAnchors( a => anchors.Add( $"{a.Abbreviation},{a.Name},{a.DefinitionFactor.ToString(CultureInfo.InvariantCulture)},{a.Definition},{a.AutoStandardPrefix}" ),
                                      f => anchors.Add( $"{f.Abbreviation},{f.Name},{f.AutoStandardPrefix}" ) );

            var text = String.Join( ';', anchors );

            // This is what does the ExtractStringAnchors extension method.
            text.Should().Be( unitPrice.ExtractStringAnchors( includeFundamentals: true ) );

            text.Should().Be( "¤,Money,None;$,Dollar,37.12,¤,Metric;g,Gram,Metric" );
        }

        [TestCase( "" )]
        [TestCase( "2^4" )]
        [TestCase( "10^-21" )]
        public void restoring_anchors_context( string expFactor )
        {
            var cA = new MeasureContext( "A" );
            var gram = cA.DefineFundamental( "g", "Gram", AutoStandardPrefix.Metric, MeasureStandardPrefix.Kilo );
            var money = cA.DefineFundamental( "¤", "Money" );
            var dollarToMoney = new FullFactor( 37.12, ExpFactor.Parse( expFactor ) );
            var dollar = cA.DefineAlias( "$", "Dollar", dollarToMoney, money, AutoStandardPrefix.Metric );

            var unitPrice = dollar / MeasureStandardPrefix.Deca.On( gram );

            var marshalled = (Anchors: unitPrice.ExtractStringAnchors( includeFundamentals: true ), Unit: unitPrice.ToString() );

            var cB = new MeasureContext( "B" );
            cB.ImportStringAnchors( marshalled.Anchors );
            cB.TryParse( marshalled.Unit, out var u ).Should().BeTrue();

            u.ToString().Should().Be( unitPrice.ToString() );
        }
    }
}

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
    public class ParseTests
    {
        [Test]
        public void parsing_unit_an_unknown_prefixed_unit_registers_it()
        {
            var ctx = new StandardMeasureContext( "Empty" );
            ctx.TryParse( "damol-8", out var u ).Should().BeTrue();
            u.ToString().Should().Be( "damol-8" );
            u.Should().BeAssignableTo<ExponentMeasureUnit>();
            (u as ExponentMeasureUnit).Exponent.Should().Be( -8 );
            var p = (u as ExponentMeasureUnit).AtomicMeasureUnit as PrefixedMeasureUnit;
            p.Prefix.Should().Be( MeasureStandardPrefix.Deca );
            p.AtomicMeasureUnit.Should().BeSameAs( ctx.Mole );
        }

        [TestCase( "s", "s" )]
        [TestCase( "s1", "s" )]
        [TestCase( "s2", "s2" )]
        [TestCase( "s0", "" )]
        [TestCase( "mm", "mm" )]
        [TestCase( "(10^-3)m", "mm" )]
        [TestCase( "(10^-3)mm", "µm" )]
        [TestCase( "(10^-22)mm", "(10^-1)ym" )]
        [TestCase( "(10^-1)kg", "hg" )]
        [TestCase( "(10^-2)kg", "dag" )]
        [TestCase( "(10^-3)kg", "g" )]
        [TestCase( "(10^-29)kg-6", "(10^-2)yg-6" )]
        [TestCase( "(10^-8*10^-12.10^-9)kg-6", "(10^-2)yg-6" )]
        public void parsing_units_finds_the_best_prefix( string text, string rewrite )
        {
            var ctx = new StandardMeasureContext( "Empty" );
            ctx.TryParse( text, out var u ).Should().BeTrue();
            u.ToString().Should().Be( rewrite );
        }

        [TestCase( "(10^-3)kg-2.mm.(10^6)K2", "MK2.mm.g-2" )]
        [TestCase( "(10^-3)kg-2*mm.(10^6)mol2", "Mmol2.mm.g-2" )]
        public void parsing_combined_units( string text, string rewrite )
        {
            var ctx = new StandardMeasureContext( "Empty" );
            ctx.TryParse( text, out var u ).Should().BeTrue();
            u.ToString().Should().Be( rewrite );
        }

        [Test]
        public void parsing_aliases()
        {
            var c = new StandardMeasureContext( "Empty" );
            var kg = c.Kilogram;
            var m = c.Metre;
            var s = c.Second;

            var newton = c.DefineAlias( "N", "Newton", FullFactor.Neutral, kg * m * (s ^ -2) );
            var dyne = c.DefineAlias( "dyn", "Dyne", new ExpFactor( 0, -5 ), newton );

            c.TryParse( "dyn-1.N", out var u ).Should().BeTrue();
            u.ToString().Should().Be( "N.dyn-1" );
            u.Should().BeSameAs( newton / dyne );
        }

        [TestCase( "10^-1", "10^-1" )]
        [TestCase( "2^7.10^-1*10^3.2^3", "10^2.2^10" )]
        [TestCase( "10^-1*2^7.10^3.2^3", "10^2.2^10" )]
        [TestCase( "%", "%" )]
        [TestCase( "10^2.%", "" )]
        [TestCase( "%.10^2", "" )]
        [TestCase( "%.‱", "10^-6" )]
        public void parsing_dimensionless_units( string text, string rewrite )
        {
            var ctx = new StandardMeasureContext( "Empty" );
            var percent = ctx.DefineAlias( "%", "Percent", new ExpFactor( 0, -2 ), MeasureUnit.None );
            var permille = ctx.DefineAlias( "‰", "Permille", new ExpFactor( 0, -3 ), MeasureUnit.None );
            var pertenthousand = ctx.DefineAlias( "‱", "Pertenthousand", new ExpFactor( 0, -4 ), MeasureUnit.None );
            ctx.TryParse( text, out var u ).Should().BeTrue();
            u.ToString().Should().Be( rewrite );
        }

    }
}

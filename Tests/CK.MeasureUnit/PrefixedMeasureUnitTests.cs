using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class PrefixedMeasureUnitTests
    {
        [Test]
        public void playing_with_decimetre_and_centimeter()
        {
            var decimetre = MeasureStandardPrefix.Deci.On( MeasureUnit.Metre );
            decimetre.Abbreviation.Should().Be( "dm" );
            decimetre.Name.Should().Be( "Decimetre" );

            var decimetreCube = decimetre ^ 3;
            decimetreCube.Abbreviation.Should().Be( "dm3" );
            decimetreCube.Name.Should().Be( "Decimetre^3" );

            // This does'nt compile and this is perfect! :)
            //var notPossible = MeasureStandardPrefix.Deci.On( decimetreCube );

            var centimetre = MeasureStandardPrefix.Centi.On( MeasureUnit.Metre );
            centimetre.Abbreviation.Should().Be( "cm" );
            centimetre.Name.Should().Be( "Centimetre" );

            var decidecimetre = MeasureStandardPrefix.Deci.On( decimetre );
            decidecimetre.Should().BeSameAs( centimetre );

            var hectocentimetre = MeasureStandardPrefix.Hecto.On( centimetre );
            hectocentimetre.Should().BeSameAs( MeasureUnit.Metre );

            var kilocentimeter = MeasureStandardPrefix.Kilo.On( centimetre );
            kilocentimeter.Abbreviation.Should().Be( "dam" );

            var decametre = MeasureStandardPrefix.Deca.On( MeasureUnit.Metre );
            decametre.Should().BeSameAs( decametre );

        }

        [Test]
        public void playing_with_adjustment_factors_with_metre()
        {
            var gigametre = MeasureStandardPrefix.Giga[MeasureUnit.Metre];
            gigametre.Abbreviation.Should().Be( "Gm" );
            gigametre.Name.Should().Be( "Gigametre" );

            var decigigametre = MeasureStandardPrefix.Deci[gigametre];
            decigigametre.Abbreviation.Should().Be( "(10^-1)Gm" );
            decigigametre.Name.Should().Be( "(10^-1)Gigametre" );

            // Instead of "(10^-2)Gigametre", we always try to minimize the absolute value of the
            // adjustement factor: here we generate the "(10^1)Megametre".
            var decidecigigametre = MeasureStandardPrefix.Deci[decigigametre];
            decidecigigametre.Abbreviation.Should().Be( "(10^1)Mm" );
            decidecigigametre.Name.Should().Be( "(10^1)Megametre" );

            var decidecidecigigametre = MeasureStandardPrefix.Deci[decidecigigametre];
            decidecidecigigametre.Abbreviation.Should().Be( "Mm" );
            decidecidecigigametre.Name.Should().Be( "Megametre" );
        }

        [Test]
        public void playing_with_adjustment_factors_with_kilogram()
        {
            var teragram = MeasureStandardPrefix.Giga[MeasureUnit.Kilogram];
            teragram.Abbreviation.Should().Be( "Tg" );
            teragram.Name.Should().Be( "Teragram" );

            var decitg = MeasureStandardPrefix.Deci[teragram];
            decitg.Abbreviation.Should().Be( "(10^-1)Tg" );
            decitg.Name.Should().Be( "(10^-1)Teragram" );

            // Instead of "(10^-2)Teragram", we always try to minimize the absolute value of the
            // adjustement factor: here we generate the "(10^1)Gigagram".
            var decidecitg = MeasureStandardPrefix.Deci[decitg];
            decidecitg.Abbreviation.Should().Be( "(10^1)Gg" );
            decidecitg.Name.Should().Be( "(10^1)Gigagram" );

            var decidecidecitg = MeasureStandardPrefix.Deci[decidecitg];
            decidecidecitg.Abbreviation.Should().Be( "Gg" );
            decidecidecitg.Name.Should().Be( "Gigagram" );
        }

        [Test]
        public void out_of_bounds_adjustment_factors_with_metre()
        {
            var yottametre = MeasureStandardPrefix.Yotta[MeasureUnit.Metre];

            var lotOfMetre = MeasureStandardPrefix.Hecto[yottametre];
            lotOfMetre.Abbreviation.Should().Be( "(10^2)Ym" );

            var evenMore = MeasureStandardPrefix.Deca[lotOfMetre];
            evenMore.Abbreviation.Should().Be( "(10^3)Ym" );

            var backToReality = MeasureStandardPrefix.Yocto[evenMore];
            backToReality.Abbreviation.Should().Be( "km" );

            var belowTheAtom = MeasureStandardPrefix.Yocto[backToReality];
            belowTheAtom.Abbreviation.Should().Be( "zm" );
            belowTheAtom.Name.Should().Be( "Zeptometre", "The Zeptometre is 10^-21 metre." );

            var decizettametre = MeasureStandardPrefix.Deci[belowTheAtom];
            decizettametre.Abbreviation.Should().Be( "(10^-1)zm" );
            var decidecizettametre = MeasureStandardPrefix.Deci[decizettametre];
            decidecizettametre.Abbreviation.Should().Be( "(10^1)ym" );

            var yoctometre = MeasureStandardPrefix.Deci[decidecizettametre];
            yoctometre.Abbreviation.Should().Be( "ym" );

            var below1 = MeasureStandardPrefix.Deci[yoctometre];
            below1.Abbreviation.Should().Be( "(10^-1)ym" );

            var below2 = MeasureStandardPrefix.Deci[below1];
            below2.Abbreviation.Should().Be( "(10^-2)ym" );

        }

        [Test]
        public void out_of_bounds_adjustment_factors_with_gram()
        {
            var yottagram = MeasureStandardPrefix.Yotta[MeasureUnit.Gram];

            var lotOfGram = MeasureStandardPrefix.Hecto[yottagram];
            lotOfGram.Abbreviation.Should().Be( "(10^2)Yg" );

            var evenMore = MeasureStandardPrefix.Deca[lotOfGram];
            evenMore.Abbreviation.Should().Be( "(10^3)Yg" );

            var backToReality = MeasureStandardPrefix.Yocto[evenMore];
            backToReality.Abbreviation.Should().Be( "kg" );

            var belowTheAtom = MeasureStandardPrefix.Yocto[backToReality];
            belowTheAtom.Abbreviation.Should().Be( "zg" );
            belowTheAtom.Name.Should().Be( "Zeptogram", "The Zeptogram is 10^-24 kilogram." );
            belowTheAtom.Normalization.Should().BeSameAs( MeasureUnit.Kilogram );
            belowTheAtom.NormalizationFactor.Should().Be( new FullFactor( new ExpFactor(0, -24) ) );

            var decizettametre = MeasureStandardPrefix.Deci[belowTheAtom];
            decizettametre.Abbreviation.Should().Be( "(10^-1)zg" );
            var decidecizettametre = MeasureStandardPrefix.Deci[decizettametre];
            decidecizettametre.Abbreviation.Should().Be( "(10^1)yg" );

            var yoctogram = MeasureStandardPrefix.Deci[decidecizettametre];
            yoctogram.Abbreviation.Should().Be( "yg" );

            var below1 = MeasureStandardPrefix.Deci[yoctogram];
            below1.Abbreviation.Should().Be( "(10^-1)yg" );

            var below2 = MeasureStandardPrefix.Deci[below1];
            below2.Abbreviation.Should().Be( "(10^-2)yg" );

        }

    }
}

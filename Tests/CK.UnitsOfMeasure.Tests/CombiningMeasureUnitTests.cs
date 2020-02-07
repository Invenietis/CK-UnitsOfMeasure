using FluentAssertions;
using System;
using NUnit.Framework;

namespace CK.UnitsOfMeasure.Tests
{

    [TestFixture]
    public class MeasureUnitTests
    {
        [Test]
        public void simple_inversion()
        {
            var am1 = MeasureUnit.Ampere.Invert();
            am1.ToString().Should().Be( "A-1" );

            var a1 = am1.Invert();
            a1.Should().BeSameAs( MeasureUnit.Ampere );

            var noneInv = MeasureUnit.None.Invert();
            noneInv.Should().BeSameAs( MeasureUnit.None );
        }

        [Test]
        public void simple_multiplication()
        {
            var a = MeasureUnit.Ampere.Multiply( MeasureUnit.Second );
            a.ToString().Should().Be( "A.s" );

            var b = a.Multiply( MeasureUnit.Kelvin );
            b.ToString().Should().Be( "A.K.s" );

            var c = b.Multiply( MeasureUnit.Second );
            c.ToString().Should().Be( "s2.A.K" );

            var d = c.Multiply( MeasureUnit.Kelvin.Invert() );
            d.ToString().Should().Be( "s2.A" );

            var e = d.Multiply( MeasureUnit.Second.Invert() );
            e.Should().BeSameAs( a );
        }


        [Test]
        public void playing_with_standard_derived_units()
        {
            // From https://en.wikipedia.org/wiki/SI_derived_unit
            // 
            // hertz        | Hz        | frequency                 | 1/s       | s−1
            // radian       | rad       | angle                     | m/m       | 1
            // steradian    | sr        | solid angle               | m2/m2     | 1
            // newton       | N         | force, weight             | kg⋅m/s2   | kg⋅m⋅s−2
            // pascal       | Pa        | pressure, stress          | N/m2      | kg⋅m−1⋅s−2
            // joule        | J         | energy, work, heat        | N⋅m       |
            //                                                      | C⋅V       |
            //                                                      | W⋅s       | kg⋅m2⋅s−2
            // watt         | W         | power, radiant flux       | J/s       |
            //                                                      | V⋅A       | kg⋅m2⋅s−3
            // coulomb      | C         | quantity of electricity   | s⋅A       |
            //                                                      | F⋅V       | s⋅A
            // volt         | V         | voltage                   | W/A       |
            //                                                      | J/C       | kg⋅m2⋅s−3⋅A−1
            // farad        | F         | electrical capacitance    | C/V       | 
            //                                                      | s/Ω       | kg−1⋅m−2⋅s4⋅A2
            // ohm          | Ω         | electrical impedance      | 1/S       | 
            //                                                      | V/A       | kg⋅m2⋅s−3⋅A−2
            // siemens      | S         | electrical conductance    | 1/Ω       |
            //                                                      | A/V       | kg−1⋅m−2⋅s3⋅A2
            // weber        | Wb        | magnetic flux             | J/A       |
            //                                                      | T⋅m2      | kg⋅m2⋅s−2⋅A−1
            // tesla        | T         | magnetic field strength   | V⋅s/m2    |
            //                                                      | Wb/m2     |
            //                                                      | N/(A⋅m)	| kg⋅s−2⋅A−1
            // henry        | H         | electrical inductance     | V⋅s/A     |
            //                                                      | Ω⋅s       |
            //                                                      | Wb/A      | kg⋅m2⋅s−2⋅A−2
            // lumen        | lm        | luminous flux             | cd⋅sr     | cd
            // lux          | lx        | illuminance               | lm/m2     | m−2⋅cd
            // becquerel    | Bq        | radioactivity             | 1/s       | s−1
            // gray         | Gy        | absorbed dose             | J/kg      | m2⋅s−2
            // sievert      | Sv        | dose ionizing radiation   | J/kg      | m2⋅s−2
            // katal        | kat       | catalytic activity        | mol/s     | s−1⋅mol

            var metre = MeasureUnit.Metre;
            var second = MeasureUnit.Second;
            var kilogram = MeasureUnit.Kilogram;
            var ampere = MeasureUnit.Ampere;
            var candela = MeasureUnit.Candela;
            var squaredMeter = metre^2;
            squaredMeter.Abbreviation.Should().Be( "m2" );

            var hertz = MeasureUnit.None / second;
            hertz.Abbreviation.Should().Be( "s-1" );

            var rad = metre * (metre ^ -1);
            rad.Should().BeSameAs( MeasureUnit.None );

            var steradian = squaredMeter / squaredMeter;
            steradian.Should().BeSameAs( MeasureUnit.None );

            var newton = kilogram * metre * (second ^ -2);
            newton.Abbreviation.Should().Be( "kg.m.s-2" );

            var pascal = newton / squaredMeter;
            pascal.Abbreviation.Should().Be( "kg.m-1.s-2" );

            var joule = newton * metre;
            var watt = joule / second;
            var coulomb = ampere * second;

            var volt = watt / ampere;
            var volt2 = joule / coulomb;
            volt.Should().BeSameAs( volt2 );
            volt.Abbreviation.Should().Be( "m2.kg.A-1.s-3" );

            var farad = coulomb / volt;
            farad.Abbreviation.Should().Be( "s4.A2.kg-1.m-2" );

            var ohm = volt / ampere;
            ohm.Abbreviation.Should().Be( "m2.kg.A-2.s-3" );

            var farad2 = second / ohm;
            farad2.Should().BeSameAs( farad );

            var siemens = MeasureUnit.None / ohm;
            var siemens2 = ampere / volt;
            siemens.Should().BeSameAs( siemens2 );
            siemens.Abbreviation.Should().Be( "s3.A2.kg-1.m-2" );

            var weber = joule / ampere;
            var tesla = volt * second / squaredMeter;
            var tesla2 = weber / squaredMeter;
            var tesla3 = newton / (ampere * metre);
            tesla2.Should().BeSameAs( tesla );
            tesla3.Should().BeSameAs( tesla );
            tesla.Abbreviation.Should().Be( "kg.A-1.s-2" );

            var henry = ohm * second;
            var henry2 = volt * second / ampere;
            var henry3 = weber / ampere;
            henry2.Should().BeSameAs( henry );
            henry3.Should().BeSameAs( henry );
            henry.Abbreviation.Should().Be( "m2.kg.A-2.s-2" );

            var lumen = candela * steradian;
            var lux = lumen / squaredMeter;
            lux.Abbreviation.Should().Be( "cd.m-2" );

        }

        [Test]
        public void combining_same_dimension()
        {
            var metre = MeasureUnit.Metre;
            var decimetre = MeasureStandardPrefix.Deci[ metre ];
            var centimetre = MeasureStandardPrefix.Centi[ metre ];

            var r10 = metre.DivideBy( decimetre );
            r10.Normalization.Should().Be( MeasureUnit.None );
            r10.NormalizationFactor.Factor.Should().Be( 1.0 );
            r10.NormalizationFactor.ExpFactor.Exp10.Should().Be( 1 );
            r10.NormalizationFactor.ExpFactor.Exp2.Should().Be( 0 );

            var inch = MeasureUnit.DefineAlias( "in", "Inch", 2.54, centimetre );
            var rInch = inch / centimetre;
            rInch.Normalization.Should().Be( MeasureUnit.None );
            rInch.NormalizationFactor.Factor.Should().Be( 2.54 );
            rInch.NormalizationFactor.ExpFactor.Exp10.Should().Be( 0 );
            rInch.NormalizationFactor.ExpFactor.Exp2.Should().Be( 0 );
        }
    }
}

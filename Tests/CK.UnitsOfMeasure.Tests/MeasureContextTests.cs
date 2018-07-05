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

            another.Invoking( c => c.DefineAlias( "derived", "Derived", 2, kilogram))
                .Should().Throw<Exception>().Where( ex => ex.Message.Contains( "Context mismatch" ) );

            StandardMeasureContext.Default.Invoking( c => c.DefineAlias( "derived", "Derived", 2, anotherKilogram ) )
                .Should().Throw<Exception>().Where( ex => ex.Message.Contains( "Context mismatch" ) );


            Action a = () => Console.WriteLine( kilogram / anotherKilogram );
            a.Should().Throw<Exception>().Where( ex => ex.Message.Contains( "Context mismatch" ) );
        }
    }
}

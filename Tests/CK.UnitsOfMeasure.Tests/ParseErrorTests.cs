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
    public class ParseErrorTests
    {
        [TestCase( "mmm" )]
        [TestCase( "kkg" )]
        [TestCase( "MMK" )]
        public void more_than_one_standard_prefix_is_not_allowed( string invalid )
        {
            var ctx = new StandardMeasureContext( "Empty" );
            ctx.TryParse( invalid, out var u ).Should().BeFalse();
            u.Should().BeNull();
        }

        [TestCase( "a" )]
        [TestCase( "k" )]
        [TestCase( "Gthing" )]
        public void unknown_units( string invalid )
        {
            var ctx = new StandardMeasureContext( "Empty" );
            ctx.TryParse( invalid, out var u ).Should().BeFalse();
            u.Should().BeNull();
        }

    }
}

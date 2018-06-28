using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    public static class MeasureUnitExtension
    {

        public static Quantity WithUnit( this int @this, MeasureUnit u ) => new Quantity( @this, u );

        public static Quantity WithUnit( this double @this, MeasureUnit u ) => new Quantity( @this, u );

    }
}

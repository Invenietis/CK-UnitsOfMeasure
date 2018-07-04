using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    public static class MeasureUnitExtension
    {
        public static MeasureUnit NullSafe( this MeasureUnit @this ) => @this == null ? MeasureUnit.None : @this;

        public static Quantity WithUnit( this int @this, MeasureUnit u ) => new Quantity( @this, u );

        public static Quantity WithUnit( this double @this, MeasureUnit u ) => new Quantity( @this, u );

    }
}

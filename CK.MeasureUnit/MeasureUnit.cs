using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CK.Core
{
    /// <summary>
    /// Base class for all measure unit. 
    /// </summary>
    public abstract partial class MeasureUnit
    {
        protected MeasureUnit( string abbreviation, string name )
        {
            Abbreviation = abbreviation;
            Name = name;
        }

        public string Abbreviation { get; }

        public string Name { get; }

        public string ToString( bool withName ) => withName ? $"{Abbreviation} ({Name})" : Abbreviation;

        public override string ToString() => Abbreviation;

    }
}

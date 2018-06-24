using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;

namespace CK.Core
{
    /// <summary>
    /// A fundamental measure unit is semantically bound to a dimension, and can be identified by its <see cref="Abbreviation"/>
    /// or by its <see cref="LongName"/>. 
    /// See http://en.wikipedia.org/wiki/SI_base_unit.
    /// </summary>
    public class FundamentalMeasureUnit : BasicMeasureUnit, IComparable<FundamentalMeasureUnit>
    {


        ///// <summary>
        ///// A byte is now standardized as eight bits, as documented in ISO/IEC 2382-1:1993.
        ///// The international standard IEC 80000-13 codified this common meaning.
        ///// Associated trait is "B".
        ///// </summary>
        //public static readonly CKTrait Byte;


        internal FundamentalMeasureUnit( string abbreviation, string name )
            : base(abbreviation, name)
        {
        }

        public BasicMeasureUnit WithExponent( int exp )
        {
            if (exp == 0) return None;
            if (exp == 1) return this;
            return RegisterBasic(exp, this);
        }

        public int CompareTo(FundamentalMeasureUnit other)
        {
            return other == null ? 1 : Abbreviation.CompareTo(other.Abbreviation);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="BasicMeasureUnit"/> is an exponentiation of a <see cref="FundamentalMeasureUnit"/> (like "m^3" or "s^-1").
    /// </summary>
    public class BasicMeasureUnit : NormalizedMeasureUnit, IComparable<BasicMeasureUnit>
    {
        internal BasicMeasureUnit((string A, string L) names, int exp, FundamentalMeasureUnit u)
            : base(names.A, names.L)
        {
            Debug.Assert(exp != 0);
            Exponent = exp;
            MeasureUnit = u;
        }

        /// <summary>
        /// Constructor for <see cref="FundamentalMeasureUnit"/>
        /// </summary>
        /// <param name="abbreviation">The abbreviation.</param>
        /// <param name="name">The full name.</param>
        private protected BasicMeasureUnit(string abbreviation, string name)
            : base(abbreviation, name)
        {
            Exponent = 1;
            MeasureUnit = (FundamentalMeasureUnit)this;
        }

        public int Exponent { get; }

        public FundamentalMeasureUnit MeasureUnit { get; }

        public static (string A, string N) ComputeNames(int exp, FundamentalMeasureUnit u)
        {
            if (exp == 1) return (u.Abbreviation, u.Name);
            string e = exp.ToString();
            return (u.Abbreviation + e, u.Name + e);
        }

        public int CompareTo(BasicMeasureUnit other)
        {
            if (other == null) return 1;
            int cmp = Exponent.CompareTo(other.Exponent);
            return cmp == 0 ? MeasureUnit.CompareTo(other.MeasureUnit) : -cmp;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// A <see cref="ExponentMeasureUnit"/> is an exponentiation of a <see cref="Core.AtomicMeasureUnit"/>
    /// (like "m^3", "s^-1", "Pa^3" or "dm2").
    /// </summary>
    public class ExponentMeasureUnit : CombinedMeasureUnit, IComparable<ExponentMeasureUnit>
    {
        internal ExponentMeasureUnit((string A, string N) names, int exp, AtomicMeasureUnit u)
            : base(names.A, names.N)
        {
            Debug.Assert(exp != 0);
            Exponent = exp;
            AtomicMeasureUnit = u;
        }

        /// <summary>
        /// Constructor for <see cref="FundamentalMeasureUnit"/>
        /// </summary>
        /// <param name="abbreviation">The abbreviation.</param>
        /// <param name="name">The full name.</param>
        private protected ExponentMeasureUnit(string abbreviation, string name)
            : base(abbreviation, name)
        {
            Exponent = 1;
            AtomicMeasureUnit = (AtomicMeasureUnit)this;
        }

        public int Exponent { get; }

        public AtomicMeasureUnit AtomicMeasureUnit { get; }

        public static (string A, string N) ComputeNames(int exp, AtomicMeasureUnit u)
        {
            if (exp == 1) return (u.Abbreviation, u.Name);
            string e = exp.ToString();
            return (u.Abbreviation + e, u.Name + "^" + e);
        }

        public int CompareTo(ExponentMeasureUnit other)
        {
            if (other == null) return 1;
            int cmp = Exponent.CompareTo(other.Exponent);
            return cmp == 0 ? AtomicMeasureUnit.CompareTo(other.AtomicMeasureUnit) : -cmp;
        }
    }
}

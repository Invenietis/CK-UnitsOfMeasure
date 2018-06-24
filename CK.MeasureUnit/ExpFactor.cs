using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    public struct ExpFactor : IComparable<ExpFactor>, IEquatable<ExpFactor>
    {
        /// <summary>
        /// The empty factor (0,0).
        /// </summary>
        public static readonly ExpFactor Empty;

        public readonly int Exp2;
        public readonly int Exp10;

        public bool IsEmpty => Exp2 == 0 && Exp10 == 0;

        public ExpFactor(int exp2, int exp10)
        {
            Exp2 = exp2;
            Exp10 = exp10;
        }

        public ExpFactor Pow(int p) => new ExpFactor(Exp2 * p, Exp10 * p);

        public ExpFactor Mult(ExpFactor x) => new ExpFactor(Exp2 + x.Exp2, Exp10 + x.Exp10);


        public double ToDouble() => Math.Pow(2, Exp2) * Math.Pow(10, Exp10);

        public override string ToString()
        {
            string s = s = Exp10 > 0 ? $"^{Exp10}" : String.Empty;
            if (Exp2 > 0) s += $"²{Exp2}";
            return s;
        }

        public override bool Equals(object obj) => obj is ExpFactor f && Equals(f);

        public bool Equals(ExpFactor other) => Exp2 == other.Exp2 && Exp10 == other.Exp10;

        public override int GetHashCode() => Exp10 << 7 ^ Exp2;

        public int CompareTo(ExpFactor other)
        {
            int cmp = Exp10 - other.Exp10;
            return cmp == 0 ? Exp2 - other.Exp2 : cmp;
        }

    }
}

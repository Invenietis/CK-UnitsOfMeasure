using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Represents a standard prefix like "Kilo" (abbrev. "k") or "Mega" (abbrev. "M")
    /// or binary prefix like "Kibi" (abbrev. "ki"). 
    /// See http://en.wikipedia.org/wiki/Metric_prefix and https://en.wikipedia.org/wiki/Binary_prefix.
    public sealed class MeasureStandardPrefix
    {
        /// <summary>
        /// Gets the abbreviation of the prefix.
        /// </summary>
        public string Abbreviation { get; }

        /// <summary>
        /// Gets the full name of the prefix.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the base: <see cref="MeasurePrefixKind.Metric"/> = 10 for metric prefixes
        /// ("k (Kilo)", "Mega (M)", etc.), <see cref="MeasurePrefixKind.Binary"/> = 2 for binary prefixes 
        /// ("Ki (Kibi)", "Mi (Mibi)", etc.).
        /// </summary>
        public MeasurePrefixKind Kind { get; }

        /// <summary>
        /// Gets the prefix exponent: 3 for "k (Kilo)", 10 for "Ki (Kibi)", 6 for "M (Mega)", 20 for "Mi (Mebi)", etc.
        /// </summary>
        public int Exponent { get; }

        /// <summary>
        /// Gets the <see cref="ExpFactor"/> of this prefix.
        /// </summary>
        public ExpFactor Factor { get; }

        public string ToString(bool withName) => withName ? $"{Abbreviation} ({Name}, {Factor})" : Abbreviation;

        public override string ToString() => Abbreviation;

        MeasureStandardPrefix(string abbreviation, string name, MeasurePrefixKind k, int exp)
        {
            Abbreviation = abbreviation;
            Name = name;
            Kind = k;
            Exponent = exp;
            Factor = k == MeasurePrefixKind.Binary ? new ExpFactor(exp, 0) : new ExpFactor(0, exp);
            _prefixes.Add(abbreviation, this);
            if (name.Length > 0) _prefixes.Add(name, this);
        }


        private static readonly Dictionary<string, MeasureStandardPrefix> _prefixes = new Dictionary<string, MeasureStandardPrefix>();

        /// <summary>
        /// None prefix. It has empty <see cref="Abbreviation"/> and <see cref="Name"/>, its <see cref="Kind"/> is <see cref="MeasurePrefixKind.None"/>, 
        /// <see cref="Exponent"/> is 0 and <see cref="Factor"/> is <see cref="ExpFactor.Empty"/>.
        /// </summary>
        public static readonly MeasureStandardPrefix None = new MeasureStandardPrefix("", "", MeasurePrefixKind.None, 0);

        public static readonly MeasureStandardPrefix Yotta = new MeasureStandardPrefix("Y", "Yotta", MeasurePrefixKind.Metric, 24);
        public static readonly MeasureStandardPrefix Zetta = new MeasureStandardPrefix("Z", "Zetta", MeasurePrefixKind.Metric, 21);
        public static readonly MeasureStandardPrefix Exa = new MeasureStandardPrefix("E", "Exa", MeasurePrefixKind.Metric, 18);
        public static readonly MeasureStandardPrefix Peta = new MeasureStandardPrefix("P", "Peta", MeasurePrefixKind.Metric, 15);
        public static readonly MeasureStandardPrefix Tera = new MeasureStandardPrefix("T", "Tera", MeasurePrefixKind.Metric, 12);
        public static readonly MeasureStandardPrefix Giga = new MeasureStandardPrefix("G", "Giga", MeasurePrefixKind.Metric, 9);
        public static readonly MeasureStandardPrefix Mega = new MeasureStandardPrefix("M", "Mega", MeasurePrefixKind.Metric, 6);
        public static readonly MeasureStandardPrefix Kilo = new MeasureStandardPrefix("k", "Kilo", MeasurePrefixKind.Metric, 3);
        public static readonly MeasureStandardPrefix Hecto = new MeasureStandardPrefix("h", "Hecto", MeasurePrefixKind.Metric, 2);
        public static readonly MeasureStandardPrefix Deca = new MeasureStandardPrefix("da", "Deca", MeasurePrefixKind.Metric, 1);
        public static readonly MeasureStandardPrefix Deci = new MeasureStandardPrefix("d", "Deci", MeasurePrefixKind.Metric, -1);
        public static readonly MeasureStandardPrefix Centi = new MeasureStandardPrefix("c", "Centi", MeasurePrefixKind.Metric, -2);
        public static readonly MeasureStandardPrefix Milli = new MeasureStandardPrefix("m", "Milli", MeasurePrefixKind.Metric, -3);
        public static readonly MeasureStandardPrefix Micro = new MeasureStandardPrefix("µ", "Micro", MeasurePrefixKind.Metric, -6);
        public static readonly MeasureStandardPrefix Nano = new MeasureStandardPrefix("n", "Nano", MeasurePrefixKind.Metric, -9);
        public static readonly MeasureStandardPrefix Pico = new MeasureStandardPrefix("p", "Pico", MeasurePrefixKind.Metric, -12);
        public static readonly MeasureStandardPrefix Femto = new MeasureStandardPrefix("f", "Femto", MeasurePrefixKind.Metric, -15);
        public static readonly MeasureStandardPrefix Atto = new MeasureStandardPrefix("a", "Atto", MeasurePrefixKind.Metric, -18);
        public static readonly MeasureStandardPrefix Zepto = new MeasureStandardPrefix("z", "Zepto", MeasurePrefixKind.Metric, -21);
        public static readonly MeasureStandardPrefix Yocto = new MeasureStandardPrefix("y", "Yocto", MeasurePrefixKind.Metric, -24);

        public static readonly MeasureStandardPrefix Yobi = new MeasureStandardPrefix("Yi", "Yobi", MeasurePrefixKind.Binary, 80);
        public static readonly MeasureStandardPrefix Zebi = new MeasureStandardPrefix("Zi", "Zebi", MeasurePrefixKind.Binary, 70);
        public static readonly MeasureStandardPrefix Exbi = new MeasureStandardPrefix("Ei", "Exbi", MeasurePrefixKind.Binary, 60);
        public static readonly MeasureStandardPrefix Pebi = new MeasureStandardPrefix("Pi", "Pebi", MeasurePrefixKind.Binary, 50);
        public static readonly MeasureStandardPrefix Tebi = new MeasureStandardPrefix("Ti", "Tebi", MeasurePrefixKind.Binary, 40);
        public static readonly MeasureStandardPrefix Gibi = new MeasureStandardPrefix("Gi", "Gibi", MeasurePrefixKind.Binary, 30);
        public static readonly MeasureStandardPrefix Mebi = new MeasureStandardPrefix("Mi", "Mebi", MeasurePrefixKind.Binary, 20);
        public static readonly MeasureStandardPrefix Kibi = new MeasureStandardPrefix("Ki", "Kibi", MeasurePrefixKind.Binary, 10);

        /// <summary>
        /// Gets a prefix by its abbreviation or name.
        /// </summary>
        /// <param name="n">The abbreviation or name of the prefix.</param>
        /// <returns>The associated prefix, if no prefix was found, <c>null</c>.</returns>
        public static MeasureStandardPrefix Get(string n)
        {
            _prefixes.TryGetValue(n, out var p);
            return p;
        }

        /// <summary>
        /// Gets all standard prefixes.
        /// </summary>
        public static IReadOnlyCollection<MeasureStandardPrefix> All => _prefixes.Values;
    }
}


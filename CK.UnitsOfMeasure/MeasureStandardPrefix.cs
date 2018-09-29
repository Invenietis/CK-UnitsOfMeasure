using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.UnitsOfMeasure
{
    /// <summary>
    /// Represents a standard prefix like "Kilo" (abbrev. "k") or "Mega" (abbrev. "M")
    /// or binary prefix like "Kibi" (abbrev. "ki").
    /// All standard prefix are registered in a static dictionary and there is no way to add
    /// new prefix.
    /// See http://en.wikipedia.org/wiki/Metric_prefix and https://en.wikipedia.org/wiki/Binary_prefix.
    /// </summary>
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
        /// Gets the base: 10 for metric prefixes ("k (Kilo)", "Mega (M)", etc.),
        /// 2 for binary prefixes ("Ki (Kibi)", "Mi (Mibi)", etc.) and 0 for <see cref="MeasureUnit.None"/>.
        /// </summary>
        public int Base { get; }

        /// <summary>
        /// Gets the prefix exponent: 3 for "k (Kilo)", 10 for "Ki (Kibi)", 6 for "M (Mega)", 20 for "Mi (Mebi)", etc.
        /// </summary>
        public int Exponent { get; }

        /// <summary>
        /// Gets the <see cref="ExpFactor"/> of this prefix.
        /// </summary>
        public ExpFactor Factor { get; }

        /// <summary>
        /// Returs the <see cref="Abbreviation"/> or the Abbreviation followed by (<see cref="Name"/>, <see cref="Factor"/>).
        /// </summary>
        /// <param name="withName">True if name and factor should be added.</param>
        /// <returns>The readable string.</returns>
        public string ToString( bool withName ) => withName ? $"{Abbreviation} ({Name}, {Factor})" : Abbreviation;

        /// <summary>
        /// Simply returns the <see cref="Abbreviation"/>.
        /// </summary>
        /// <returns>The abbreviation.</returns>
        public override string ToString() => Abbreviation;

        MeasureStandardPrefix( string abbreviation, string name, int eBase, int exp )
        {
            Abbreviation = abbreviation;
            Name = name;
            Base = eBase;
            Exponent = exp;
            Factor = eBase == 2 ? new ExpFactor( exp, 0 ) : new ExpFactor( 0, exp );
        }

        /// <summary>
        /// Applies this metric to a <see cref="AtomicMeasureUnit"/>.
        /// </summary>
        /// <param name="unit">The original unit.</param>
        /// <returns>The resulting unit.</returns>
        public AtomicMeasureUnit this[ AtomicMeasureUnit unit ] => Combine( this, unit, true ).Result;

        /// <summary>
        /// Applies this metric to a <see cref="AtomicMeasureUnit"/>.
        /// </summary>
        /// <param name="unit">The original unit.</param>
        /// <returns>The resulting unit.</returns>
        public AtomicMeasureUnit On( AtomicMeasureUnit unit ) => Combine( this, unit, true ).Result;

        /// <summary>
        /// Applies this metric to a <see cref="AtomicMeasureUnit"/>, allowing the creation of
        /// potentially adjusted <see cref="PrefixedMeasureUnit"/>.
        /// </summary>
        /// <param name="unit">The original unit.</param>
        /// <param name="allowAdjustmentPrefix">
        /// When true, this is the same as calling <see cref="this[AtomicMeasureUnit]"/>: the resulting <see cref="PrefixedMeasureUnit"/>
        /// can have a non neutral <see cref="PrefixedMeasureUnit.AdjustmentFactor"/>.
        /// When false, if there is the need of an adjusment prefix (for instance on a "DeciMega") the adjustement will be returned and
        /// the resulting AtomicMeasureUnit will have a neutral adjustment.
        /// </param>
        /// <returns>The resulting adjustment and unit.</returns>
        public (ExpFactor Adjustment, AtomicMeasureUnit Result) On( AtomicMeasureUnit unit, bool allowAdjustmentPrefix ) => Combine( this, unit, allowAdjustmentPrefix );

        static (ExpFactor Adjustment, AtomicMeasureUnit Result) Combine( MeasureStandardPrefix prefix, AtomicMeasureUnit u, bool allowAdjustmentPrefix )
        {
            if( prefix.Base == 0 ) return (ExpFactor.Neutral, u);
            if( u is PrefixedMeasureUnit p )
            {
                var newExp = prefix.Factor.Multiply( p.Prefix.Factor ).Multiply( p.AdjustmentFactor );
                if( newExp.IsNeutral ) return (ExpFactor.Neutral,p.AtomicMeasureUnit);
                var best = FindBest( newExp, p.AtomicMeasureUnit.AutoStandardPrefix );
                if( !allowAdjustmentPrefix ) return (best.Adjustment, u.Context.RegisterPrefixed( ExpFactor.Neutral, best.Prefix, p.AtomicMeasureUnit ));
                return (ExpFactor.Neutral, u.Context.RegisterPrefixed( best.Adjustment, best.Prefix, p.AtomicMeasureUnit ));
            }
            if( (u.AutoStandardPrefix & AutoStandardPrefix.Binary) == 0 && prefix.Base == 2 
                || (u.AutoStandardPrefix & AutoStandardPrefix.Metric) == 0 && prefix.Base == 10 )
            {
                    return (ExpFactor.Neutral, u.Context.RegisterPrefixed( prefix.Factor, None, u ));
            }
            return (ExpFactor.Neutral, u.Context.RegisterPrefixed( ExpFactor.Neutral, prefix, u ));
        }

        /// <summary>
        /// Finds the best prefix given a <see cref="ExpFactor"/>.
        /// If both metric and binary exponents exist (ie. are not zero), the metric prefix
        /// is selected (the Exp2 is injected in the adjustment factor).
        /// </summary>
        /// <param name="newExp">Must not be the neutral factor.</param>
        /// <param name="stdPrefix">
        /// Whether the actual standard prefixes are allowed. When not, it is the adjustemnt factor
        /// that handles the exponent and the prefix None is used.
        /// </param>
        /// <returns>The best prefix with the required adjustment factor.</returns>
        internal static (ExpFactor Adjustment, MeasureStandardPrefix Prefix) FindBest( ExpFactor newExp, AutoStandardPrefix stdPrefix )
        {
            // Privilegiates metric prefix if any and if metric is allowed.
            Debug.Assert( !newExp.IsNeutral );
            if( newExp.Exp10 != 0 && (stdPrefix&AutoStandardPrefix.Metric) != 0 )
            {
                var b10 = FindBest( newExp.Exp10, _allMetricIndex, _allMetric );
                return (new ExpFactor(newExp.Exp2, b10.Item1), b10.Item2);
            }
            if( newExp.Exp2 != 0 && (stdPrefix&AutoStandardPrefix.Binary) != 0 )
            {
                Debug.Assert( newExp.Exp2 != 0 );
                var b2 = FindBest( newExp.Exp2, _allBinaryIndex, _allBinary );
                return (new ExpFactor( b2.Item1, 0 ), b2.Item2);
            }
            return (newExp, None);
        }

        static (int,MeasureStandardPrefix) FindBest( int v, int[] indexes, MeasureStandardPrefix[] prefixes )
        {
            int idx = Array.BinarySearch( indexes, v );
            if( idx >= 0 ) return (0, prefixes[idx]);
            idx = ~idx;
            if( idx == indexes.Length ) --idx;
            else if( idx != 0 )
            {
                var positiveDelta = prefixes[idx].Exponent - v;
                Debug.Assert( positiveDelta > 0 );
                var fromPrevious = v - prefixes[idx - 1].Exponent;
                Debug.Assert( fromPrevious > 0 );
                if( fromPrevious < positiveDelta ) --idx;
            }
            var p = prefixes[idx];
            return (v-p.Exponent,p);
        }

        private static readonly Dictionary<string, MeasureStandardPrefix> _prefixes;
        private static readonly MeasureStandardPrefix[] _allBinary;
        private static readonly int[] _allBinaryIndex;
        private static readonly MeasureStandardPrefix[] _allMetric;
        private static readonly int[] _allMetricIndex;

        static MeasureStandardPrefix()
        {
            None = new MeasureStandardPrefix( "", "", 0, 0 );

            Yocto = new MeasureStandardPrefix( "y", "Yocto", 10, -24 );
            Zepto = new MeasureStandardPrefix( "z", "Zepto", 10, -21 );
            Atto = new MeasureStandardPrefix( "a", "Atto", 10, -18 );
            Femto = new MeasureStandardPrefix( "f", "Femto", 10, -15 );
            Pico = new MeasureStandardPrefix( "p", "Pico", 10, -12 );
            Nano = new MeasureStandardPrefix( "n", "Nano", 10, -9 );
            Micro = new MeasureStandardPrefix( "µ", "Micro", 10, -6 );
            Milli = new MeasureStandardPrefix( "m", "Milli", 10, -3 );
            Centi = new MeasureStandardPrefix( "c", "Centi", 10, -2 );
            Deci = new MeasureStandardPrefix( "d", "Deci", 10, -1 );
            Deca = new MeasureStandardPrefix( "da", "Deca", 10, 1 );
            Hecto = new MeasureStandardPrefix( "h", "Hecto", 10, 2 );
            Kilo = new MeasureStandardPrefix( "k", "Kilo", 10, 3 );
            Mega = new MeasureStandardPrefix( "M", "Mega", 10, 6 );
            Giga = new MeasureStandardPrefix( "G", "Giga", 10, 9 );
            Tera = new MeasureStandardPrefix( "T", "Tera", 10, 12 );
            Peta = new MeasureStandardPrefix( "P", "Peta", 10, 15 );
            Exa = new MeasureStandardPrefix( "E", "Exa", 10, 18 );
            Zetta = new MeasureStandardPrefix( "Z", "Zetta", 10, 21 );
            Yotta = new MeasureStandardPrefix( "Y", "Yotta", 10, 24 );

            Kibi = new MeasureStandardPrefix( "Ki", "Kibi", 2, 10 );
            Mebi = new MeasureStandardPrefix( "Mi", "Mebi", 2, 20 );
            Gibi = new MeasureStandardPrefix( "Gi", "Gibi", 2, 30 );
            Tebi = new MeasureStandardPrefix( "Ti", "Tebi", 2, 40 );
            Pebi = new MeasureStandardPrefix( "Pi", "Pebi", 2, 50 );
            Exbi = new MeasureStandardPrefix( "Ei", "Exbi", 2, 60 );
            Zebi = new MeasureStandardPrefix( "Zi", "Zebi", 2, 70 );
            Yobi = new MeasureStandardPrefix( "Yi", "Yobi", 2, 80 );

            _allMetric = new MeasureStandardPrefix[] {
                Yocto, Zepto, Atto, Femto, Pico, Nano, Micro, Milli, Centi, Deci, Deca,
                Hecto, Kilo, Mega, Giga, Tera, Peta, Exa, Zetta, Yotta };
            _allMetricIndex = _allMetric.Select( p => (int)p.Factor.Exp10 ).ToArray();

            _allBinary = new MeasureStandardPrefix[] { Kibi, Mebi, Gibi, Tebi, Pebi, Exbi, Zebi, Yobi };
            _allBinaryIndex = _allBinary.Select( p => (int)p.Factor.Exp2 ).ToArray();

            _prefixes = new Dictionary<string, MeasureStandardPrefix>();
            _prefixes.Add( String.Empty, None );
            foreach( var p in _allMetric.Concat( _allBinary ) )
            {
                _prefixes.Add( p.Abbreviation, p );
                _prefixes.Add( p.Name, p );
            }
        }

        /// <summary>
        /// None prefix. It has empty <see cref="Abbreviation"/> and <see cref="Name"/>, its <see cref="Base"/> is 0, 
        /// <see cref="Exponent"/> is 0 and <see cref="Factor"/> is <see cref="ExpFactor.Neutral"/>.
        /// </summary>
        public static readonly MeasureStandardPrefix None;

#pragma warning disable 1591
        public static readonly MeasureStandardPrefix Yotta;
        public static readonly MeasureStandardPrefix Zetta;
        public static readonly MeasureStandardPrefix Exa;
        public static readonly MeasureStandardPrefix Peta;
        public static readonly MeasureStandardPrefix Tera;
        public static readonly MeasureStandardPrefix Giga;
        public static readonly MeasureStandardPrefix Mega;
        public static readonly MeasureStandardPrefix Kilo;
        public static readonly MeasureStandardPrefix Hecto;
        public static readonly MeasureStandardPrefix Deca;
        public static readonly MeasureStandardPrefix Deci;
        public static readonly MeasureStandardPrefix Centi;
        public static readonly MeasureStandardPrefix Milli;
        public static readonly MeasureStandardPrefix Micro;
        public static readonly MeasureStandardPrefix Nano;
        public static readonly MeasureStandardPrefix Pico;
        public static readonly MeasureStandardPrefix Femto;
        public static readonly MeasureStandardPrefix Atto;
        public static readonly MeasureStandardPrefix Zepto;
        public static readonly MeasureStandardPrefix Yocto;

        public static readonly MeasureStandardPrefix Yobi;
        public static readonly MeasureStandardPrefix Zebi;
        public static readonly MeasureStandardPrefix Exbi;
        public static readonly MeasureStandardPrefix Pebi;
        public static readonly MeasureStandardPrefix Tebi;
        public static readonly MeasureStandardPrefix Gibi;
        public static readonly MeasureStandardPrefix Mebi;
        public static readonly MeasureStandardPrefix Kibi;
#pragma warning restore 1591

        /// <summary>
        /// Gets a prefix by its abbreviation or name.
        /// </summary>
        /// <param name="n">The abbreviation or name of the prefix.</param>
        /// <returns>The associated prefix, if no prefix was found, <c>null</c>.</returns>
        public static MeasureStandardPrefix Get( string n )
        {
            _prefixes.TryGetValue( n, out var p );
            return p;
        }

        /// <summary>
        /// Gets all standard prefixes (including <see cref="None"/>).
        /// </summary>
        public static IEnumerable<MeasureStandardPrefix> All => _allMetric.Concat( _allBinary );

        /// <summary>
        /// Gets all the metric prefixes.
        /// </summary>
        public static IReadOnlyCollection<MeasureStandardPrefix> MetricPrefixes => _allMetric;

        /// <summary>
        /// Gets all the binary prefixes.
        /// </summary>
        public static IReadOnlyCollection<MeasureStandardPrefix> BinaryPrefixes => _allBinary;

        /// <summary>
        /// Gets all the prefixes for a <see cref="AutoStandardPrefix"/>.
        /// </summary>
        /// <param name="a">The auto standard prefix.</param>
        /// <returns>An empty set, <see cref="All"/>, <see cref="MetricPrefixes"/> or <see cref="BinaryPrefixes"/>.</returns>
        public static IEnumerable<MeasureStandardPrefix> GetPrefixes( AutoStandardPrefix a )
        {
            switch( a )
            {
                case AutoStandardPrefix.Both: return All;
                case AutoStandardPrefix.Binary: return BinaryPrefixes;
                case AutoStandardPrefix.Metric: return MetricPrefixes;
            }
            return Enumerable.Empty<MeasureStandardPrefix>();
        }

        /// <summary>
        /// Based on the first or first and second charaters of the given string, tries to find
        /// the corresponding <see cref="Abbreviation"/>.
        /// </summary>
        /// <param name="s">The string to search.</param>
        /// <returns>The standard prefix or <see cref="None"/> if not found.</returns>
        public static MeasureStandardPrefix FindPrefix( string s )
        {
            if( String.IsNullOrWhiteSpace( s ) ) return None;
            switch( s[0] )
            {
                case 'y': return Yocto;
                case 'z': return Zepto;
                case 'a': return Atto;
                case 'f': return Femto;
                case 'p': return Pico;
                case 'n': return Nano;
                case 'µ': return Micro;
                case 'm': return Milli;
                case 'c': return Centi;
                case 'd': return s.Length > 1 && s[1] == 'a' ? Deca : Deci;
                case 'h': return Hecto;
                case 'k': return Kilo;
                case 'K': return s.Length > 1 && s[1] == 'i' ? Kibi : None;
                case 'M': return s.Length > 1 && s[1] == 'i' ? Mebi : Mega;
                case 'G': return s.Length > 1 && s[1] == 'i' ? Gibi : Giga;
                case 'T': return s.Length > 1 && s[1] == 'i' ? Tebi : Tera;
                case 'P': return s.Length > 1 && s[1] == 'i' ? Pebi : Peta;
                case 'E': return s.Length > 1 && s[1] == 'i' ? Exbi : Exa;
                case 'Z': return s.Length > 1 && s[1] == 'i' ? Zebi : Zetta;
                case 'Y': return s.Length > 1 && s[1] == 'i' ? Yobi : Yotta;
            }
            return None;
        }
    }

}


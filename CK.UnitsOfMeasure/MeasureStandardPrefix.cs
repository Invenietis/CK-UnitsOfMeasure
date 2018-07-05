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

        public string ToString( bool withName ) => withName ? $"{Abbreviation} ({Name}, {Factor})" : Abbreviation;

        public override string ToString() => Abbreviation;

        MeasureStandardPrefix( string abbreviation, string name, MeasurePrefixKind k, int exp )
        {
            Abbreviation = abbreviation;
            Name = name;
            Kind = k;
            Exponent = exp;
            Factor = k == MeasurePrefixKind.Binary ? new ExpFactor( exp, 0 ) : new ExpFactor( 0, exp );
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
        /// Applies this metric to a <see cref="AtomicMeasureUnit"/>, allowing the creation of adjusted
        /// <see cref="PrefixedMeasureUnit"/> or not.
        /// </summary>
        /// <param name="unit">The original unit.</param>
        /// <param name="allowAdjustmentPrefix">
        /// When true, this is the same as calling <see cref="On(AtomicMeasureUnit)"/>: the resulting <see cref="PrefixedMeasureUnit"/>
        /// can have a non neutral <see cref="PrefixedMeasureUnit.AdjustmentFactor"/>.
        /// When false, if there is the need of an adjusment prefix (for instance on a "DeciMega") the adjustement will be returned and
        /// the resulting AtomicMeasureUnit will have a neutral adjustment.
        /// </param>
        /// <returns>The resulting adjustment and unit.</returns>
        public (ExpFactor Adjustment, AtomicMeasureUnit Result) On( AtomicMeasureUnit unit, bool allowAdjustmentPrefix ) => Combine( this, unit, true );

        static (ExpFactor Adjustment, AtomicMeasureUnit Result) Combine( MeasureStandardPrefix prefix, AtomicMeasureUnit u, bool allowAdjustmentPrefix )
        {
            if( prefix.Kind == MeasurePrefixKind.None ) return (ExpFactor.Neutral, u);
            if( u is PrefixedMeasureUnit p )
            {
                var newExp = prefix.Factor.Multiply( p.Prefix.Factor ).Multiply( p.AdjustmentFactor );
                if( newExp.IsNeutral ) return (ExpFactor.Neutral,p.AtomicMeasureUnit);
                var best = FindBest( newExp );
                if( !allowAdjustmentPrefix ) return (best.Adjustment, u.Context.RegisterPrefixed( ExpFactor.Neutral, best.Prefix, p.AtomicMeasureUnit ));
                return (ExpFactor.Neutral, u.Context.RegisterPrefixed( best.Adjustment, best.Prefix, p.AtomicMeasureUnit ));
            }
            return (ExpFactor.Neutral, u.Context.RegisterPrefixed( ExpFactor.Neutral, prefix, u ));
        }

        static (ExpFactor Adjustment, MeasureStandardPrefix Prefix) FindBest( ExpFactor newExp )
        {
            // Privilegiates metric prefix if any.
            Debug.Assert( !newExp.IsNeutral );
            if( newExp.Exp10 != 0 )
            {
                var b10 = FindBest( newExp.Exp10, _allMetricIndex, _allMetric );
                return (new ExpFactor(newExp.Exp2, b10.Item1), b10.Item2);
            }
            Debug.Assert( newExp.Exp2 != 0 );
            var b2 = FindBest( newExp.Exp2, _allBinaryIndex, _allBinary );
            return (new ExpFactor(b2.Item1,0), b2.Item2);
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
            None = new MeasureStandardPrefix( "", "", MeasurePrefixKind.None, 0 );

            Yocto = new MeasureStandardPrefix( "y", "Yocto", MeasurePrefixKind.Metric, -24 );
            Zepto = new MeasureStandardPrefix( "z", "Zepto", MeasurePrefixKind.Metric, -21 );
            Atto = new MeasureStandardPrefix( "a", "Atto", MeasurePrefixKind.Metric, -18 );
            Femto = new MeasureStandardPrefix( "f", "Femto", MeasurePrefixKind.Metric, -15 );
            Pico = new MeasureStandardPrefix( "p", "Pico", MeasurePrefixKind.Metric, -12 );
            Nano = new MeasureStandardPrefix( "n", "Nano", MeasurePrefixKind.Metric, -9 );
            Micro = new MeasureStandardPrefix( "µ", "Micro", MeasurePrefixKind.Metric, -6 );
            Milli = new MeasureStandardPrefix( "m", "Milli", MeasurePrefixKind.Metric, -3 );
            Centi = new MeasureStandardPrefix( "c", "Centi", MeasurePrefixKind.Metric, -2 );
            Deci = new MeasureStandardPrefix( "d", "Deci", MeasurePrefixKind.Metric, -1 );
            Deca = new MeasureStandardPrefix( "da", "Deca", MeasurePrefixKind.Metric, 1 );
            Hecto = new MeasureStandardPrefix( "h", "Hecto", MeasurePrefixKind.Metric, 2 );
            Kilo = new MeasureStandardPrefix( "k", "Kilo", MeasurePrefixKind.Metric, 3 );
            Mega = new MeasureStandardPrefix( "M", "Mega", MeasurePrefixKind.Metric, 6 );
            Giga = new MeasureStandardPrefix( "G", "Giga", MeasurePrefixKind.Metric, 9 );
            Tera = new MeasureStandardPrefix( "T", "Tera", MeasurePrefixKind.Metric, 12 );
            Peta = new MeasureStandardPrefix( "P", "Peta", MeasurePrefixKind.Metric, 15 );
            Exa = new MeasureStandardPrefix( "E", "Exa", MeasurePrefixKind.Metric, 18 );
            Zetta = new MeasureStandardPrefix( "Z", "Zetta", MeasurePrefixKind.Metric, 21 );
            Yotta = new MeasureStandardPrefix( "Y", "Yotta", MeasurePrefixKind.Metric, 24 );

            Kibi = new MeasureStandardPrefix( "Ki", "Kibi", MeasurePrefixKind.Binary, 10 );
            Mebi = new MeasureStandardPrefix( "Mi", "Mebi", MeasurePrefixKind.Binary, 20 );
            Gibi = new MeasureStandardPrefix( "Gi", "Gibi", MeasurePrefixKind.Binary, 30 );
            Tebi = new MeasureStandardPrefix( "Ti", "Tebi", MeasurePrefixKind.Binary, 40 );
            Pebi = new MeasureStandardPrefix( "Pi", "Pebi", MeasurePrefixKind.Binary, 50 );
            Exbi = new MeasureStandardPrefix( "Ei", "Exbi", MeasurePrefixKind.Binary, 60 );
            Zebi = new MeasureStandardPrefix( "Zi", "Zebi", MeasurePrefixKind.Binary, 70 );
            Yobi = new MeasureStandardPrefix( "Yi", "Yobi", MeasurePrefixKind.Binary, 80 );

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
        /// None prefix. It has empty <see cref="Abbreviation"/> and <see cref="Name"/>, its <see cref="Kind"/> is <see cref="MeasurePrefixKind.None"/>, 
        /// <see cref="Exponent"/> is 0 and <see cref="Factor"/> is <see cref="ExpFactor.Neutral"/>.
        /// </summary>
        public static readonly MeasureStandardPrefix None;

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
        public static IReadOnlyCollection<MeasureStandardPrefix> All => _prefixes.Values;

        /// <summary>
        /// Gets all the metric prefixes.
        /// </summary>
        public static IReadOnlyCollection<MeasureStandardPrefix> MetricPrefixes => _allMetric;

        /// <summary>
        /// Gets all the binary prefixes.
        /// </summary>
        public static IReadOnlyCollection<MeasureStandardPrefix> BinaryPrefixes => _allBinary;
    }
}


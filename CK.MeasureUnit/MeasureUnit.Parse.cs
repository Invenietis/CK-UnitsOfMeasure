using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace CK.Core
{
    public partial class MeasureUnit
    {

        //static Regex _rUnit = new Regex( @"(?<1>[^\.\^]+)(\^(?<2>-?\d+))?", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        //public static MeasureUnit Parse( string s )
        //{
        //    // 1 - Normalize input by removing all white spaces.
        //    // Trust the Regex cache for this quite common one.
        //    s = Regex.Replace(s, "\\s+", String.Empty);

        //    // 2 - Obtain the list of Abbreviation[^Exp].
        //    (string Abb, int Exp) Map( Match m )
        //    {
        //        var num = m.Captures[2].Value;
        //        int e = num.Length > 0 ? Int32.Parse(num) : 1;
        //        return (m.Captures[1].Value, e );
        //    }

        //    _rUnit.Matches( s ).Cast<Match>().Select( Map )

        //    m.MatchWord()
        //    MeasureUnit result = null;
        //    var parts = s.Split('.');
        //    if( s.Length == 1 )
        //    {
        //        if (_units.TryGetValue(s, out result)) return result;
        //        var m = new StringMatcher(s);
        //        int idx = s.IndexOfAny(new[] { '^', '²' });
        //        if (idx <= 0) return null;
        //        if( )
        //    }
        //}

    }
}

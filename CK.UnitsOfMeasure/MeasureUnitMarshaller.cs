using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CK.UnitsOfMeasure
{
    public static class MeasureUnitMarshaller
    {
        public static void ExtractAnchors( this MeasureUnit unit, Action<AliasMeasureUnit> alias, Action<FundamentalMeasureUnit> fundamental )
        {
            switch( unit )
            {
                case FundamentalMeasureUnit f:
                    if( f != MeasureUnit.None ) fundamental?.Invoke( f );
                    break;
                case AliasMeasureUnit a:
                    ExtractAnchors( a.Definition, alias, fundamental );
                    alias.Invoke( a );
                    break;
                // This is NOT an error: PrefixedMeasureUnit specializes ExponentMeasureUnit and masks the AtomicMeasureUnit to be the "Definition" of the
                // prefixed unit.
                // At the ExponentMeasureUnit level, the AtomicMeasureUnit is itself: "dag" (exponent 1) is actually the "dag" 
                case PrefixedMeasureUnit p:
                    ExtractAnchors( p.AtomicMeasureUnit, alias, fundamental );
                    break;
                case ExponentMeasureUnit e:
                    ExtractAnchors( e.AtomicMeasureUnit, alias, fundamental );
                    break;
                default:
                    foreach( var u in unit.MeasureUnits)
                    {
                        ExtractAnchors( u, alias, fundamental );
                    }
                    break;
            }
        }

        public static string ExtractStringAnchors( this MeasureUnit unit, bool includeFundamentals = true )
        {
            StringBuilder b = new StringBuilder();
            unit.ExtractAnchors( AppendAlias,
                                 includeFundamentals ? AppendFundamental : null );

            void AppendAlias( AliasMeasureUnit a )
            {
                if( b.Length > 0 ) b.Append( ';' );
                b.Append( a.Abbreviation ).Append( ',' )
                 .Append( a.Name ).Append( ',' )
                 .Append( a.DefinitionFactor.ToString( CultureInfo.InvariantCulture ) ).Append( ',' )
                 .Append( a.Definition ).Append( ',' )
                 .Append( a.AutoStandardPrefix );
            }

            void AppendFundamental( FundamentalMeasureUnit f )
            {
                if( b.Length > 0 ) b.Append( ';' );
                b.Append( f.Abbreviation ).Append( ',' )
                 .Append( f.Name ).Append( ',' )
                 .Append( f.AutoStandardPrefix );
            }

            return b.ToString();
        }

        public static void ImportStringAnchors( this MeasureContext context, string anchors )
        {
            foreach( var a in anchors.Split( ';' ) )
            {
                var t = a.Split( ',' );
                if( t.Length > 2 )
                {
                    if( t.Length == 3 )
                    {
                        context.DefineFundamental( t[0], t[1], (AutoStandardPrefix)Enum.Parse( typeof( AutoStandardPrefix ), t[2] ) );
                        continue;
                    }
                    if( t.Length == 5 )
                    {
                        var f = FullFactor.Parse( t[2], CultureInfo.InvariantCulture );
                        var def = context[t[3]];
                        if( def == null ) throw new Exception( $"Unable to resolve abbreviation '{t[3]}'." );
                        context.DefineAlias( t[0], t[1], f, def, (AutoStandardPrefix)Enum.Parse( typeof( AutoStandardPrefix ), t[4] ) );
                        continue;
                    }
                    throw new Exception( $"Invalid anchor definition: '{t}'." );
                }
            }
        }

    }
}

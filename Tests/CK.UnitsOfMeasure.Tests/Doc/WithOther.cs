using CK.UnitsOfMeasure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.MeasureUnit.Tests.Doc
{
    class M
    {
        private protected M( string name )
        {
            Name = name;
        }

        public string Name { get; }

        public static readonly F None = new F( "None" );
        public static readonly F Kilogram = new F( "Kilogram" );
    }

    class N<T> : M where T : M
    {
        readonly B<T>[] _units;

        public N( B<T>[] units ) : base( "Name is built from the units array." )
        {
            _units = units;
        }
        private protected N( string name ) : base( name )
        {
            _units = new[] { (B<T>)this };
        }

        //public bool WeNeedNoneHere()
        //{
        //    _units[0] = None;
        //    return _units.Contains( None );
        //}
    }

    class B<T> : N<T> where T : M
    {
        public B( int exp, T fundamental ) : base( "Name is built from fundamental and exp." )
        {
            Exponent = exp;
            Measure = fundamental;
        }

        private protected B( string name ) : base( name )
        {
            Exponent = 1;
            Measure = (T)(object)this;
        }

        public int Exponent { get; }
        public T Measure { get; }
    }

    class F : B<F>
    {
        public F( string name ) : base( name )
        {
        }
    }

    class O : B<O>
    {
        /// <summary>
        /// Full definition of another unit.
        /// This can handle for instance:
        /// - newton: N ≡ 1 kg⋅m/s2 that is a direct alias to a
        ///   normalized unit.
        ///   var newton = O( "N", 1.0, ExprFactor.Empty, MeasureUnit.Kilogram * MeasureUnit.Metre / MeasaureUnit.Second.Power(2) )
        /// - 1 dyn	= 10−5 N
        ///   var dyn = O( "dyn", 1.0, new ExprFactor(0,-5), newton.Normalized )
        /// - 1 kp	= 9.80665 N
        ///   var kp = O( "kp", 9.80665, ExprFactor.Empty, newton.Normalized )
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factor"></param>
        /// <param name="e"></param>
        /// <param name="normalized"></param>
        public O( string name, double factor, ExpFactor e, N<F> normalized ) : base( name )
        {
            Factor = factor;
            ExpFactor = e;
            Normalized = normalized;
        }

        public O( MeasureStandardPrefix p, F f ) : base( p.Abbreviation + f.Name )
        {
            Factor = 1.0;
            ExpFactor = p.Factor;
            Normalized = f;
        }

        public O( MeasureStandardPrefix p, O o ) : base( p.Abbreviation + o.Name )
        {
            Factor = o.Factor;
            ExpFactor = p.Factor.Multiply( o.ExpFactor );
            Normalized = o.Normalized;
        }

        public double Factor { get; }
        public ExpFactor ExpFactor { get; }

        public N<F> Normalized { get; }
    }


}

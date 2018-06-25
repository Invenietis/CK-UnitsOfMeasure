using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.MeasureUnit.Tests.Doc
{
    class NonGenericV1
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

        class N : M
        {
            readonly B[] _units;

            public N( B[] units ) : base( "Name is built from the units array." )
            {
                _units = units;
            }
            private protected N( string name ) : base( name )
            {
                _units = new[] { (B)this };
            }

            public bool WeNeedNoneHere()
            {
                _units[0] = None;
                return _units.Contains( None );
            }
        }

        class B : N
        {
            public B( int exp, A atomic ) : base( "Name is built from fundamental and exp." )
            {
                Exponent = exp;
                Atomic = atomic;
            }

            private protected B( string name ) : base( name )
            {
                Exponent = 1;
                Atomic = (A)this;
            }

            public int Exponent { get; }
            public A Atomic { get; }

        }

        class A : B
        {
            public A( string name ) : base( name )
            {
            }
        }


        class F : A
        {
            public F( string name ) : base( name )
            {
            }
        }

        class D : A
        {
            public D( string name, double factor, ExpFactor e, N normalized ) : base( name )
            {
            }
        }

    }
}

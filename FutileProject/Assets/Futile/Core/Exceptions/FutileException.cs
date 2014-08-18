using System;

namespace Futile.Core.Exceptions
{
    public class FutileException : Exception
    {
        public FutileException( string message ) : base( message )
        {
        }

        public FutileException()
        {
        }
    }
}
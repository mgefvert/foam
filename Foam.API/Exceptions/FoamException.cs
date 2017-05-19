using System;

namespace Foam.API.Exceptions
{
    public class FoamException : Exception
    {
        public FoamException(string message) : base(message)
        {
        }
    }
}

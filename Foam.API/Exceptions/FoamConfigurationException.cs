using System;

namespace Foam.API.Exceptions
{
    public class FoamConfigurationException : FoamException
    {
        public FoamConfigurationException(string message) : base(message)
        {
        }
    }
}

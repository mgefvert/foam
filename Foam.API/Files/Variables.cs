using System;
using System.Collections.Generic;

namespace Foam.API.Files
{
    public class Variables : Dictionary<string, string>
    {
        public Variables() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        public Variables(IDictionary<string, string> variables) : base(variables, StringComparer.InvariantCultureIgnoreCase)
        {
        }
    }
}

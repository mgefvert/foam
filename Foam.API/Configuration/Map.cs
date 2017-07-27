using System;
using System.Collections.Generic;

namespace Foam.API.Configuration
{
    public class Map : Dictionary<string, string>
    {
        public string Name { get; set; }

        public Map() : base(StringComparer.CurrentCultureIgnoreCase)
        {
        }
    }
}

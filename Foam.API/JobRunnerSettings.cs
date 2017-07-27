using System;
using System.Collections.Generic;
using Foam.API.Configuration;
using Foam.API.Memory;

namespace Foam.API
{
    public class JobRunnerSettings
    {
        public JobDefinition Definition { get; set; }
        public ExtensionLibrary Library { get; set; }
        public IMemory Memory { get; set; }
        public IEnumerable<Map> Maps { get; set; }
    }
}

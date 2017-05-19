using System;
using System.Collections.Generic;
using DotNetCommons;

namespace Foam
{
    public class Options
    {
        [CommandLineOption('c', "config", "Configuration file to use.")]
        public string ConfigFile { get; set; } = "foam.xml";

        [CommandLineOption('d', "debug", "Use debug logging.")]
        public bool Debug { get; set; }

        [CommandLineOption("commands")]
        public bool ListCommands { get; set; }

        [CommandLineRemaining]
        public List<string> Arguments { get; } = new List<string>();
    }
}

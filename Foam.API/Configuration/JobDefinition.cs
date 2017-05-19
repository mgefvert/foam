using Foam.API.Commands;
using System;
using System.Collections.Generic;

namespace Foam.API.Configuration
{
    public class JobDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<ICommand> Commands { get; } = new List<ICommand>();
    }
}

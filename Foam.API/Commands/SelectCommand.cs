using System;
using Foam.API.Attributes;

namespace Foam.API.Commands
{
    [ShortDescription("Filters the file buffer and runs subcommands for all selected files.")]
    [LongDescription("Temporarily selects a number of files matching a particular filter. Any subcommands to this command " +
                     "will run using this filtered list. Once the command has executed, the file buffer will be restored " +
                     "to its original state.")]
    public class SelectCommand : SelectBaseCommand
    {
        [PropertyDescription("Variable to evaluate.")]
        public string Var { get; set; }
        [PropertyDescription("Type of operation to perform: eq (equal, default), neq (not equal), any (any value) or like (matches regex)")]
        public FilterOperation Op { get; set; } = FilterOperation.Eq;
        [PropertyDescription("Optional value to match against")]
        public string Value { get; set; }
    }
}

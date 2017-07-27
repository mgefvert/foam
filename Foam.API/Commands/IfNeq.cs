using System;
using System.Collections.Generic;
using System.Linq;
using Foam.API.Attributes;
using Foam.API.Exceptions;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Process subcommands for all files that has a variable that does not equal a given value.")]
    [LongDescription("Applies a filter to the file list; any subcommands to this command will be executed on any file " +
                     "which variable 'var' does not equal a specific value. If no such file exists, no subcommands will be executed.")]
    public class IfNeqCommand : ICompoundCommand
    {
        [PropertyDescription("Optional file mask specifies what files to test.")]
        public string Mask { get; set; }
        [PropertyDescription("Variable to evaluate.")]
        public string Var { get; set; }
        [PropertyDescription("Value to not match.")]
        public string Value { get; set; }

        public ICollection<ICommand> Commands { get; } = new List<ICommand>();

        public void Initialize()
        {
            if (string.IsNullOrEmpty(Var))
                throw new FoamConfigurationException("No variable defined.");
        }

        public void Execute(JobRunner runner)
        {
        }

        public FileList Filter(FileList files)
        {
            return new FileList(files.SelectFiles(Evaluator.Text(Mask))
                .Where(f => !string.Equals(Value, Evaluator.Variable(Var, f), StringComparison.CurrentCultureIgnoreCase)));
        }
    }
}

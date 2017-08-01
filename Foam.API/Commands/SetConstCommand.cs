using System;
using Foam.API.Attributes;
using Foam.API.Exceptions;

namespace Foam.API.Commands
{
    [ShortDescription("Set a job-wide constant to a specific value or regex result.")]
    [LongDescription("Set-const sets a constant to a specific text. It is useful for passing information to job subparts; the job-part " +
                     "definition automatically inherits the file buffer and any variables or constants.")]
    public class SetConstCommand : ICommand
    {
        [PropertyDescription("The value to set.")]
        public string Value { get; set; }
        [PropertyDescription("Which variable (constant) to set.")]
        public string To { get; set; }

        public void Initialize()
        {
            if (Value == null)
                throw new FoamConfigurationException("Either 'var' or 'text' must be defined.");
            if (string.IsNullOrEmpty(To))
                throw new FoamConfigurationException("No target variable 'to' is defined.");
        }

        public void Execute(JobRunner runner)
        {
            runner.Constants[To] = Value;
        }
    }
}

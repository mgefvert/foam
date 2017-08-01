using System;
using Foam.API.Attributes;
using Foam.API.Exceptions;

namespace Foam.API.Commands
{
    [ShortDescription("Call another job.")]
    [LongDescription("Calls a different job and runs it, using the current file buffer and constants as the input.")]
    public class CallCommand : ICommand
    {
        [PropertyDescription("Name of the job to call.")]
        public string Job { get; set; }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(Job))
                throw new FoamConfigurationException("Job name must be specified.");
        }

        public void Execute(JobRunner runner)
        {
            runner.Call(Job);
        }
    }
}

using System;
using Foam.API.Attributes;
using Foam.API.Exceptions;

namespace Foam.API.Commands
{
    [ShortDescription("Forces an execution failure.")]
    [LongDescription("Forces an exception with an optional message, aborting script execution and rolling back any file operations.")]
    public class FailCommand : ICommand
    {
        [PropertyDescription("Message to display.")]
        public string Message { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            throw new FoamException(Message ?? "The job forced an execution failure.");
        }
    }
}

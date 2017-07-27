using System;
using Foam.API.Attributes;
using Foam.API.Exceptions;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Forces an execution failure.")]
    [LongDescription("Forces an exception with an optional message, aborting script execution and rolling back any file operations.")]
    public class FailCommand : ICommand
    {
        [PropertyDescription("Message to display.")]
        public string Text { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            throw new FoamException(Evaluator.Text(Text) ?? "The job forced an execution failure.");
        }
    }
}

using System;
using Foam.API.Attributes;

namespace Foam.API.Commands
{
    [ShortDescription("Take a snapshot of the current file buffer.")]
    [LongDescription("Takes a snapshot of the file buffer and pushes it onto an internal stack, " +
                     "ensuring that you can return to the exact same state later on with a pop command.")]
    public class PushCommand : ICommand
    {
        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            runner.PushFileBuffer();
        }
    }
}

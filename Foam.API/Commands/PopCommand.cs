using System;
using Foam.API.Attributes;

namespace Foam.API.Commands
{
    [ShortDescription("Restore a previous snapshot of the file buffer.")]
    [LongDescription("Restores the last snapshot of the file buffer by popping it from the internal stack, " +
                     "ensuring that you return to the exact same state as was previously saved.")]
    public class PopCommand : ICommand
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

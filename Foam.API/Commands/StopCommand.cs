using System;
using Foam.API.Attributes;
using Foam.API.Exceptions;

namespace Foam.API.Commands
{
    [ShortDescription("Immediately stop job execution.")]
    [LongDescription("Immediately ends the current execution of the job as if the script ran to the end.")]
    public class Stopommand : ICommand
    {
        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            throw new FoamStopJobException();
        }
    }
}

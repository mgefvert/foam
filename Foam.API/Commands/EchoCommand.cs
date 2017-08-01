using System;
using DotNetCommons;
using Foam.API.Attributes;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Write text to screen and logging output.")]
    [LongDescription("Writes a message to the screen and also the log files for the current job.")]
    public class EchoCommand : ICommand
    {
        [PropertyDescription("Message to display.")]
        public string Text { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            Logger.Log(Evaluator.Text(Text, null, runner.Constants));
        }
    }
}

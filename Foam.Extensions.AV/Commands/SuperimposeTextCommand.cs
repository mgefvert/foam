using System;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;

namespace Foam.Extensions.AV.Commands
{
    [ShortDescription("Superimpose text over an image.")]
    [LongDescription("Superimpose text over an image.")]
    public class SuperimposeTextCommand : ICommand
    {
        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            throw new NotImplementedException();
        }
    }
}

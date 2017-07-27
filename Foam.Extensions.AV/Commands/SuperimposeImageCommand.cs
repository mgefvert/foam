using System;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;

namespace Foam.Extensions.AV.Commands
{
    [ShortDescription("Superimpose an image (PNG) over another image.")]
    [LongDescription("Superimpose an image (PNG) over another image.")]
    public class SuperimposeImageCommand : ICommand
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

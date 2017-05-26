using System;
using System.Linq;
using Foam.API.Attributes;
using Foam.API.Configuration;

namespace Foam.API.Commands
{
    [ShortDescription("Writes files to a given location.")]
    [LongDescription("The write command writes the files in the file buffer to a given location. ")]
    public class WriteCommand : ICommand
    {
        [PropertyDescription("Optional file mask specifies what files to write.")]
        public string Mask { get; set; }
        [PropertyDescription("Target location to write to.")]
        public Uri Target { get; set; }
        [PropertyDescription("Whether to overwrite files: always (default), if-newer, or never.")]
        public OverwriteMode Overwrite { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            var provider = runner.SelectProvider(Target);
            var files = runner.FileBuffer.SelectFiles(Mask).ToList();

            provider.Write(Target, files, runner.CommitBuffer, Overwrite);
        }
    }
}

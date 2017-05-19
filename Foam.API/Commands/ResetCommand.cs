using System;
using DotNetCommons;
using Foam.API.Attributes;

namespace Foam.API.Commands
{
    [ShortDescription("Clears the file buffer.")]
    [LongDescription("Clears all files from the active file buffer.")]
    public class ResetCommand : ICommand
    {
        [PropertyDescription("Optional file mask specifies what files to remove.")]
        public string Mask { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            var files = runner.FileBuffer.ExtractFiles(Mask);

            Logger.Log($"Removed {files.Count} files from the file buffer");
        }
    }
}

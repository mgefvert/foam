using System;
using System.Linq;
using DotNetCommons;
using Foam.API.Attributes;

namespace Foam.API.Commands
{
    [ShortDescription("Reads files from a given location and optionally removes them.")]
    [LongDescription("The fetch command reads files from a given location and places them in the file buffer. Upon " +
                     "completion of the job, they are automatically removed.")]
    public class FetchCommand : ICommand
    {
        [PropertyDescription("Optional file mask specifies what files to read.")]
        public string Mask { get; set; }
        [PropertyDescription("Source location to read from.")]
        public Uri Source { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            var provider = runner.SelectProvider(Source);

            var files = provider.Fetch(Source, Mask, runner.CommitBuffer);
            foreach (var file in files)
                Logger.Log($"Fetch: {file.ModifiedDate:s}  {file.Length,10:N0}  {file.Name}");

            Logger.Log($"Fetch: {files.Count} files, {files.Sum(x => x.Length)} bytes");

            runner.FileBuffer.AddRange(files);
        }
    }
}

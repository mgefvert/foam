using System;
using System.Linq;
using DotNetCommons;
using Foam.API.Attributes;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Decompress files to from archive.")]
    [LongDescription("The unpack command decompresses an archive and replaces it with the unpacked files. The command automatically " +
                     "recognizes what compression format was used.")]
    public class UnpackCommand : ICommand
    {
        [PropertyDescription("Optional file mask specifies what files to decompress.")]
        public string Mask { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            var files = runner.FileBuffer.SelectFiles(Mask).ToList();

            foreach (var file in files)
            {
                var decompressed = Compressor.Decompress(file);
                runner.FileBuffer.Remove(file);
                runner.FileBuffer.AddRangeIfNotNull(decompressed);
            }
        }
    }
}

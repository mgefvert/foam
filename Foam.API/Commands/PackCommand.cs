using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons;
using Foam.API.Attributes;
using Foam.API.Exceptions;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Compress files to an archive.")]
    [LongDescription("The pack command compresses files using a given compression format (default zip), and the resulting archive " +
                     "replaces the original file list in the file buffer. If the compression format is unable to compress several " +
                     "files into one (gzip), one archive is created per file.")]
    public class PackCommand : ICommand
    {
        [PropertyDescription("Optional file mask specifies what files to compress.")]
        public string Mask { get; set; }
        [PropertyDescription("Optional new file name. If not given, the first file compressed determines the name of the archive.")]
        public string Name { get; set; }
        [PropertyDescription("Format to use: 'gzip', 'zip', or 'sevenzip'.")]
        public CompressionMode Format { get; set; } = CompressionMode.Zip;

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            var files = runner.FileBuffer.SelectFiles(Evaluator.Text(Mask, null, runner.Constants)).ToList();

            if (Format == CompressionMode.GZip)
            {
                if (!string.IsNullOrEmpty(Name) && files.Count > 1)
                    throw new FoamException(
                        "Unable to set archive filename when multiple archives are generated. Try leaving the name parameter blank.");

                foreach (var file in files)
                {
                    var archive = Compressor.Compress(new List<FileItem> { file }, Evaluator.Text(Name, file, runner.Constants), Format);
                    runner.FileBuffer.Remove(file);
                    runner.FileBuffer.AddIfNotNull(archive);
                }
            }
            else
            {
                var archive = Compressor.Compress(files, Evaluator.Text(Name, null, runner.Constants), Format);
                foreach(var file in files)
                    runner.FileBuffer.Remove(file);
                runner.FileBuffer.AddIfNotNull(archive);
            }
        }
    }
}

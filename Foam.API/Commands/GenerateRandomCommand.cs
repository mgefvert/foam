using System;
using System.Collections.Generic;
using System.Linq;
using Foam.API.Attributes;
using Foam.API.Exceptions;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Generate random data in a new file")]
    [LongDescription("Generate random data in a new file.")]
    public class GenerateRandomCommand : ICommand
    {
        private static readonly Random Rnd = new Random();

        [PropertyDescription("Length in bytes of generated data. Default 1024 bytes.")]
        public int Len { get; set; }
        [PropertyDescription("File name to generate. If not set, a random file will be generated.")]
        public string Name { get; set; }

        public void Initialize()
        {
            if (Len < 0 || Len > 1024*1024)
                throw new FoamConfigurationException("Random data length must be between 0 bytes and 1 megabyte.");
        }

        public void Execute(JobRunner runner)
        {
            var filename = Name;
            if (string.IsNullOrEmpty(filename))
                filename = GenerateFileName(runner.FileBuffer.Select(x => x.Name));

            var buffer = new byte[Len];
            Rnd.NextBytes(buffer);
            runner.FileBuffer.Add(new FileItem(filename, DateTimeOffset.Now, buffer));
        }

        private string GenerateFileName(IEnumerable<string> existing)
        {
            var set = new HashSet<string>(existing.Select(x => x.ToLower()));

            for (var i = 0; i < 9999; i++)
            {
                var filename = "rnd-" + Rnd.Next(1, int.MaxValue);
                if (!set.Contains(filename))
                    return filename;
            }

            throw new FoamException("Unable to create a new random, unique file name.");
        }
    }
}

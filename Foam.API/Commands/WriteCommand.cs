using System;
using System.Collections.Generic;
using System.Linq;
using Foam.API.Attributes;
using Foam.API.Configuration;
using Foam.API.Files;

namespace Foam.API.Commands
{
    [ShortDescription("Writes files to a given location.")]
    [LongDescription("The write command writes the files in the file buffer to a given location. ")]
    public class WriteCommand : ICommand
    {
        [PropertyDescription("Optional file mask specifies what files to write.")]
        public string Mask { get; set; }
        [PropertyDescription("Target location to write to.")]
        public string Target { get; set; }
        [PropertyDescription("Whether to overwrite files: always (default), if-newer, or never.")]
        public OverwriteMode Overwrite { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            var groups = new Dictionary<string, FileList>();

            // Evaluate target destination from variables and group
            var files = runner.FileBuffer.SelectFiles(Evaluator.Text(Mask, null, runner.Constants)).ToList();
            foreach (var file in files)
            {
                var target = Evaluator.Text(Target, file, runner.Constants);
                if (!groups.ContainsKey(target))
                    groups[target] = new FileList();
                groups[target].Add(file);
            }

            // For each target group, select provider and write
            foreach (var group in groups)
            {
                var target = new Uri(group.Key);
                var provider = runner.SelectProvider(target);
                provider.Write(target, group.Value, runner.CommitBuffer, Overwrite);
            }
        }
    }
}

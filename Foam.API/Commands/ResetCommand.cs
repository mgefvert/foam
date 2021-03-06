﻿using System;
using DotNetCommons;
using DotNetCommons.Logger;
using Foam.API.Attributes;
using Foam.API.Files;

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
            var files = runner.FileBuffer.ExtractFiles(Evaluator.Text(Mask, null, runner.Constants));

            Logger.Log($"Removed {files.Count} files from the file buffer");
        }
    }
}

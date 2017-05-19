using System;
using System.IO;
using System.Text.RegularExpressions;
using DotNetCommons;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;
using Foam.API.Exceptions;
using Foam.API.Files;

namespace Foam.Extensions.AV.Commands
{
    [ShortDescription("Shrink MP4 files and recompress them to save space.")]
    [LongDescription("Processes MP4 files using ffmpeg and shrinks them to a maximum vertical size, along with a " +
                     "constant rate factor suitable for home video. Also corrects video rotation that's 90 degress off.")]
    public class ShrinkVideoCommand : ICommand
    {
        [PropertyDescription("Whether or not to correct video rotation. Default is true.")]
        public bool CorrectRotation { get; set; } = true;
        [PropertyDescription("Constant Rate Factor (-crf) to use. Default is 22, sane choices are 18-28. Lower is better.")]
        public int ConstantRateFactor { get; set; } = 22;
        [PropertyDescription("Frame rate to use. Default is 25; set to blank to suppress frame conversion.")]
        public int? FrameRate { get; set; } = 25;
        [PropertyDescription("File mask to use when selecting files from the file buffer.")]
        public string Mask { get; set; }
        [PropertyDescription("Maximum height of video to allow. Default is 480.")]
        public int MaxHeight { get; set; } = 480;

        public void Initialize()
        {
            foreach(var f in new[] { "ffprobe.exe", "ffmpeg.exe" })
                if (Spawn.FindExePath(f) == null)
                    throw new FoamException($"ShrinkVideoCommand: Unable to locate {f} in system path.");

            if (MaxHeight < 128)
                throw new FoamException("MaxHeight must be >= 128.");
        }

        public void Execute(JobRunner runner)
        {
            foreach(var file in runner.FileBuffer.SelectFiles(Mask))
                Logger.Catch(() => ProcessFile(file));
        }

        private void ProcessFile(FileItem file)
        {
            var temporary = file.CreateTemporaryCopy();
            try
            {
                var newfile = new FileInfo(Path.GetTempFileName());
                ShrinkVideo(temporary, newfile);
                file.LoadFromFile(newfile.FullName);

                Logger.Log($"{file.Name} shrunk from {file.Length/1048576.0:N1} MB to {newfile.Length/1048576.0:N1} MB");
            }
            finally
            {
                file.ReleaseTemporaryCopy();
            }
        }

        private void ShrinkVideo(FileInfo file, FileInfo output)
        {
            var result = Spawn.Run("ffprobe.exe", Quote(file.FullName));
            var match = Regex.Match(result, @"Video:.*?\s(\d+)x(\d+)(,|\s)", RegexOptions.Multiline);
            if (!match.Success)
                throw new Exception("Unable to interpret ffprobe results");

            var matchRotate = Regex.Match(result, "rotat.*90", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var w = int.Parse(match.Groups[1].Value);
            var h = int.Parse(match.Groups[2].Value);

            if (w < 128 || w > 4096 || h < 128 || h > 4096)
                throw new Exception("Out of range dimensions: " + w + "x" + h);

            if (matchRotate.Success)
            {
                var x = w;
                w = h;
                h = x;
            }

            string pscale = null;
            if (h > MaxHeight)
            {
                var scale = (double)MaxHeight / (matchRotate.Success ? w : h);
                h = (int)(scale * h / 4) * 4;
                w = (int)(scale * w / 4) * 4;
                pscale = "-vf scale=" + w + ":" + h;
            }

            var parameters = new[]
            {
                "-i " + Quote(file.FullName),
                "-y",
                FrameRate != null ? "-r " + FrameRate : null,
                "-crf " + ConstantRateFactor,
                pscale,
                Quote(output.FullName)
            };

            Spawn.Run("ffmpeg.exe", string.Join(" ", parameters));

            output.Refresh();
            if (!output.Exists || output.Length < 1024)
                throw new Exception("Conversion of video failed");
        }

        private static string Quote(string filename)
        {
            return '"' + filename + '"';
        }
    }
}

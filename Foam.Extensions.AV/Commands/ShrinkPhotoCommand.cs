using System;
using System.Drawing;
using DotNetCommons;
using DotNetCommons.Logger;
using DotNetCommons.WinForms.Graphics;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;
using Foam.API.Files;

namespace Foam.Extensions.AV.Commands
{
    [ShortDescription("Process images by shrinking them to a maximum pixel limit.")]
    [LongDescription("Shrinks and recompresses images to a maximum pixel limit. Even without shrinking, " +
                     "just recompressing images can yield significant savings because cameras frequently " +
                     "optimize for speed rather than compression.")]
    public class ShrinkPhotoCommand : ICommand
    {
        [PropertyDescription("File mask to operate upon.")]
        public string Mask { get; set; }
        [PropertyDescription("Maximum pixel count to allow. Default is 8388608 (8 megapixels). Set to zero to prevent resizing.")]
        public int PixelLimit { get; set; } = 8388608;

        public void Initialize()
        {
            if (PixelLimit > 0 && PixelLimit < 256)
                throw new ArgumentOutOfRangeException(nameof(PixelLimit), "Invalid pixel limit: " + PixelLimit);
        }

        public void Execute(JobRunner runner)
        {
            foreach(var file in runner.FileBuffer.SelectFiles(Mask))
                Process(file, PixelLimit);
        }

        public static void Process(FileItem file, int? pixelLimit)
        {
            if (file.Length < 1024)
                return;

            try
            {
                using (var image = Image.FromStream(file.GetStream(false)))
                {
                    var format = image.RawFormat;
                    var processor = new ImageProcessor(image);
                    var oldLen = file.Length;
                    var oldSize = processor.Size;

                    if ((pixelLimit ?? 0) > 0 && processor.PixelCount > pixelLimit)
                    {
                        var shrinkFactor = Math.Sqrt((double)pixelLimit / (processor.Width * processor.Height));
                        var w = (int)(processor.Width * shrinkFactor / 4) * 4;
                        var h = (int)(processor.Height * shrinkFactor / 4) * 4;

                        processor.Resample(w, h);
                    }

                    // Save anyway, just for better compression
                    using (var result = processor.AsStream(format))
                    {
                        var newLen = result.Length;
                        var newSize = processor.Size;

                        var pct = (int)(newLen / (float)oldLen * 100);
                        if (pct <= 1)
                        {
                            Logger.Warning($"{file.Name}: Compression with over 99%; possible malfunction. Skipping.");
                            return;
                        }

                        if (pct >= 95)
                        {
                            Logger.Log($"{file.Name}: No significant saving in size. Skipping.");
                            return;
                        }

                        file.SetData(result);
                        Logger.Log($"{file.Name}: Shrunk to {pct}% size, " +
                                   (newSize != oldSize ? $"new dimensions=({newSize.Width},{newSize.Height})" : "dimensions unchanged"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{file.Name}: {ex.Message}");
            }
        }
    }
}

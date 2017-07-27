using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DotNetCommons;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;
using Foam.API.Files;

namespace Foam.Extensions.AV.Commands
{
    public enum ConvertImageFormat
    {
        Jpg,
        Png,
        Bmp,
        Gif,
        Tiff
    }

    [ShortDescription("Convert an image to another format.")]
    [LongDescription(".")]
    public class ConvertImageCommand : ICommand
    {
        [PropertyDescription("File mask to use when selecting files from the file buffer.")]
        public string Mask { get; set; }
        [PropertyDescription("Target image format: png, jpg, bmp, gif or tiff. Default is jpg.")]
        public ConvertImageFormat Format { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            foreach (var file in runner.FileBuffer.SelectFiles(Evaluator.Text(Mask)))
                Logger.Catch(() => ConvertImage(file, Format));
        }

        public static void ConvertImage(FileItem file, ConvertImageFormat format)
        {
            using (var source = file.GetStream(false))
            using (var mem = new MemoryStream())
            using (var image = Image.FromStream(source))
            {
                image.Save(mem, GetImageFormat(format));
                file.SetData(mem.ToArray());
            }
        }

        private static ImageFormat GetImageFormat(ConvertImageFormat format)
        {
            switch (format)
            {
                case ConvertImageFormat.Png:
                    return ImageFormat.Png;
                case ConvertImageFormat.Bmp:
                    return ImageFormat.Bmp;
                case ConvertImageFormat.Gif:
                    return ImageFormat.Gif;
                case ConvertImageFormat.Tiff:
                    return ImageFormat.Tiff;
                case ConvertImageFormat.Jpg:
                    return ImageFormat.Jpeg;
                default:
                    return ImageFormat.Jpeg;
            }
        }
    }
}

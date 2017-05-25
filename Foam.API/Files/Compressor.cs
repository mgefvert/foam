using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Foam.API.Exceptions;
using SevenZip;

namespace Foam.API.Files
{
    public enum CompressionMode
    {
        GZip,
        Zip,
        SevenZip
    }

    public static class Compressor
    {
        static Compressor()
        {
            var dllname = Environment.Is64BitProcess ? "7z64.dll" : "7z32.dll";
            SevenZipBase.SetLibraryPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", dllname));
        }

        private static string ApplyDefaultExtension(string filename, CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.GZip:
                    return filename + ".gz";

                case CompressionMode.Zip:
                    return Path.ChangeExtension(filename, "zip");

                case CompressionMode.SevenZip:
                    return Path.ChangeExtension(filename, "7z");

                default:
                    throw new FoamException("Unrecognized compression mode " + mode);
            }
        }

        private static OutArchiveFormat CompressionModeToArchiveFormat(CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.GZip:
                    return OutArchiveFormat.GZip;

                case CompressionMode.Zip:
                    return OutArchiveFormat.Zip;

                case CompressionMode.SevenZip:
                    return OutArchiveFormat.SevenZip;

                default:
                    throw new FoamException("Unrecognized compression mode " + mode);
            }
        }

        public static FileItem Compress(IList<FileItem> files, string compressedName, CompressionMode mode)
        {
            if (!files.Any())
                return null;

            var compressor = new SevenZipCompressor
            {
                ArchiveFormat = CompressionModeToArchiveFormat(mode)
            };

            if (string.IsNullOrEmpty(compressedName))
                compressedName = ApplyDefaultExtension(files[0].Name, mode);

            var streams = files.ToDictionary(x => x.Name, x => x.GetStream(false));
            using (var target = new MemoryStream())
            {
                compressor.CompressStreamDictionary(streams, target);
                return new FileItem(compressedName, DateTimeOffset.Now, DateTimeOffset.Now, target.ToArray());
            }
        }

        public static FileList Decompress(FileItem file)
        {
            var result = new FileList();
            using (var source = file.GetStream(false))
            { 
                var extractor = new SevenZipExtractor(source);
                var items = extractor.ArchiveFileData;

                for (var i = 0; i < items.Count; i++)
                {
                    using (var target = new MemoryStream())
                    {
                        extractor.ExtractFile(i, target);
                        result.Add(new FileItem(items[i].FileName, DateTimeOffset.Now, target.ToArray()));
                    }
                }
            }

            return result;
        }
    }
}

using System;
using System.IO;
using System.Text;
using DotNetCommons;
using Foam.API;
using Foam.API.Attributes;
using Foam.API.Commands;
using Foam.API.Files;

namespace Foam.Extensions.AV.Commands
{
    [ShortDescription("Fix the creation date in MP4 files.")]
    [LongDescription("Uses the file creation date to set the MP4 creation date tag, if this is blank.")]
    public class FixMp4CreationDateCommand : ICommand
    {
        private static readonly DateTime Epoch = new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [PropertyDescription("Force overwriting the MP4 data, even if a date already exists.")]
        public bool Force { get; set; }
        [PropertyDescription("File mask to use when selecting files.")]
        public string Mask { get; set; }

        public void Initialize()
        {
        }

        public void Execute(JobRunner runner)
        {
            foreach (var file in runner.FileBuffer.SelectFiles(Mask))
                Logger.Catch(() => ProcessFile(file));
        }

        private void ProcessFile(FileItem file)
        {
            if (file.Data.GetText(4, 4, Encoding.Default) != "ftyp")
            {
                Logger.Warn($"File {file.Name} has a unrecognized header, likely not an MP4 file");
                return;
            }

            using (var fs = file.GetStream(false))
            using (var reader = new BinaryReader(fs))
            {
                var result = ProcessChunks(file, fs, reader, fs.Length);
                if (result != null)
                    file.SetData(result);
            }
        }

        private Stream ProcessChunks(FileItem file, Stream stream, BinaryReader reader, long endPosition)
        {
            while (stream.Position < endPosition)
            {
                var pos = stream.Position;
                var size = ReverseUInt(reader.ReadUInt32());
                if (size == 0)
                    break;

                var type = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (type == "moov")
                    return ProcessChunks(file, stream, reader, pos + size);

                if (type == "mvhd")
                {
                    var version = (int)reader.ReadByte();
                    reader.ReadBytes(3);

                    var creationTime = ReadTime(stream, version);
                    if (creationTime != null && Force == false)
                        return null;

                    var foundPosition = stream.Position - 4;

                    // Make a copy of the file
                    stream = file.GetStream(true);
                    stream.Position = foundPosition;
                    WriteTime(stream, version, file.CreationDate.UtcDateTime);

                    stream.Position = stream.Position - 4;
                    Logger.Log(file.Name + " time changed to " + ReadTime(stream, version));
                    return stream;
                }

                stream.Seek(pos + size, SeekOrigin.Begin);
            }

            return null;
        }

        private static uint ReverseUInt(uint uvalue)
        {
            uvalue = ((uvalue << 8) & 0xFF00FF00) | ((uvalue >> 8) & 0xFF00FF);
            return (uvalue << 16) | ((uvalue >> 16) & 0xFFFF);
        }

        private static DateTime? ReadTime(Stream stream, int version)
        {
            if (version == 0)
                return ReadVer0Time(stream);

            if (version == 1)
                return ReadVer1Time(stream);

            throw new Exception("Invalid version tag: " + version);
        }

        private static DateTime? ReadVer0Time(Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            Array.Reverse(buffer);
            var secs = BitConverter.ToUInt32(buffer, 0);
            return secs == 0 ? (DateTime?)null : Epoch.AddSeconds(secs);
        }

        private static DateTime? ReadVer1Time(Stream stream)
        {
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            Array.Reverse(buffer);
            var secs = BitConverter.ToUInt64(buffer, 0);
            return Epoch.AddSeconds(secs);
        }

        private static void WriteTime(Stream stream, int version, DateTime time)
        {
            if (version == 0)
            {
                WriteVer0Time(stream, time);
                return;
            }

            if (version == 1)
            {
                WriteVer1Time(stream, time);
                return;
            }

            throw new Exception("Invalid version tag: " + version);
        }

        private static void WriteVer0Time(Stream stream, DateTime time)
        {
            var buffer = BitConverter.GetBytes((uint)(time.ToUniversalTime() - Epoch).TotalSeconds);
            Array.Reverse(buffer);
            stream.Write(buffer, 0, 4);
        }

        private static void WriteVer1Time(Stream stream, DateTime time)
        {
            var buffer = BitConverter.GetBytes((ulong)(time.ToUniversalTime() - Epoch).TotalSeconds);
            Array.Reverse(buffer);
            stream.Write(buffer, 0, 8);
        }
    }
}

using System;
using System.IO;
using System.Text;
using Foam.API.Exceptions;

namespace Foam.API.Files
{
    public class FileItem : IDisposable
    {
        public string Name { get; set; }
        public string OriginalFullName { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public ReadOnlyByteBuffer Data { get; private set; }

        public int Crc32 => Data.CalcCrc32();
        public int Length => Data.Length;

        public Stream GetStream(bool writable) => writable ? Data.GetWriteableCopyStream() : Data.GetReadOnlyStream();
        public string GetString(Encoding encoding) => Data.GetString(encoding);
        public override string ToString() => $"{Timestamp:G}  {Length,10:N0}  {Name}";

        private FileInfo _temporary;

        public FileItem()
        {
        }

        public FileItem(string filename) : this(new FileInfo(filename))
        {
        }

        public FileItem(FileInfo fileinfo)
        {
            Name = fileinfo.Name;
            OriginalFullName = fileinfo.FullName;
            Timestamp = fileinfo.LastWriteTime;
            Data = new ReadOnlyByteBuffer(File.ReadAllBytes(OriginalFullName));
        }

        public FileItem(string fullname, DateTimeOffset timestamp, Stream stream)
        {
            Name = Path.GetFileName(fullname);
            OriginalFullName = fullname;
            Timestamp = timestamp;
            Data = new ReadOnlyByteBuffer(stream);
        }

        public FileItem(string fullname, DateTimeOffset timestamp, byte[] data)
        {
            Name = Path.GetFileName(fullname);
            OriginalFullName = fullname;
            Timestamp = timestamp;
            Data = new ReadOnlyByteBuffer(data);
        }

        public FileItem(string fullname, DateTimeOffset timestamp, string data, Encoding encoding = null)
            : this(fullname, timestamp, (encoding ?? Encoding.UTF8).GetBytes(data))
        {
        }

        ~FileItem()
        {
            Dispose();
        }

        public void Dispose()
        {
            ReleaseTemporaryCopy();
        }

        public FileInfo CreateTemporaryCopy()
        {
            var filename = Path.GetTempFileName();
            _temporary = new FileInfo(filename);

            using (var fs = _temporary.Create())
                WriteTo(fs);

            return _temporary;
        }

        public void LoadFromFile(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
                SetData(fs);
        }

        public void ReleaseTemporaryCopy()
        {
            if (_temporary == null)
                return;

            _temporary.Delete();
            _temporary = null;
        }

        public void RestoreFromTemporaryCopy()
        {
            if (_temporary == null)
                return;

            if (!_temporary.Exists)
                throw new FoamException($"File {Name}'s temporary backing file {_temporary.Name} no longer exists.");

            using (var fs = _temporary.OpenRead())
                SetData(fs);

            _temporary.Delete();
            _temporary = null;
        }

        public void SetData(byte[] data)
        {
            Data = new ReadOnlyByteBuffer(data);
        }

        public void SetData(Stream stream)
        {
            Data = new ReadOnlyByteBuffer(stream);
        }

        public void WriteTo(Stream stream)
        {
            using (var mem = GetStream(false))
                mem.CopyTo(stream);
        }
    }
}

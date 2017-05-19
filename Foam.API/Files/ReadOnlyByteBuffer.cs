using System;
using System.IO;
using System.Text;

namespace Foam.API.Files
{
    public class ReadOnlyByteBuffer
    {
        private readonly byte[] _data;

        public byte this[int i] => _data[i];
        public int Length => _data.Length;

        protected ReadOnlyByteBuffer()
        {
        }

        public ReadOnlyByteBuffer(byte[] data)
        {
            _data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, _data, 0, _data.Length);
        }

        public ReadOnlyByteBuffer(Stream stream)
        {
            var len = (int)(stream.Length - stream.Position);
            _data = new byte[len];
            stream.Read(_data, 0, len);
        }

        public byte[] GetData(int position, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(_data, position, result, 0, length);
            return result;
        }

        public string GetText(int position, int length, Encoding encoding)
        {
            return encoding.GetString(GetData(position, length));
        }

        public Stream GetReadOnlyStream()
        {
            return new MemoryStream(_data, false);
        }

        public Stream GetWriteableCopyStream()
        {
            var result = new MemoryStream();
            result.Write(_data, 0, _data.Length);
            result.Position = 0;
            return result;
        }
    }
}

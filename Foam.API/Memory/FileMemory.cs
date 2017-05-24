using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using DotNetCommons;

namespace Foam.API.Memory
{
    public class FileMemory : IMemory
    {
        private readonly object _lock = new object();

        private class FileObject
        {
            public string Facility { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public DateTime Updated { get; set; }

            public string Key => Facility + "\x01" + Name;
        }

        private FileStream _fs;
        private readonly Dictionary<string, FileObject> _records = new Dictionary<string, FileObject>(StringComparer.CurrentCultureIgnoreCase);
        private bool _dirty;

        public void Dispose()
        {
            lock (_lock)
            {
                if (_fs == null)
                    return;

                if (_dirty)
                    WriteData();

                _fs.Dispose();
                _fs = null;
            }
        }

        private void WriteData()
        {
            _fs.SetLength(0);

            using (var buf = new BufferedStream(_fs, 65536))
            using (var writer = new BinaryWriter(buf, Encoding.UTF8))
            {
                foreach(var obj in _records.Values)
                {
                    writer.Write(obj.Facility);
                    writer.Write(obj.Name);
                    writer.Write(obj.Value);
                    writer.Write(obj.Updated.Ticks);
                }
            }
        }

        public FileMemory(string filename)
        {
            _fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            using (var buf = new BufferedStream(_fs, 65536))
            using (var reader = new BinaryReader(buf, Encoding.UTF8))
            {
                while (buf.Position < buf.Length)
                {
                    var obj = new FileObject
                    {
                        Facility = reader.ReadString(),
                        Name = reader.ReadString(),
                        Value = reader.ReadString(),
                        Updated = DateTime.FromBinary(reader.ReadInt64())
                    };

                    _records[obj.Key] = obj;
                }
            }
        }

        public void Delete(string facility, string name)
        {
            lock (_lock)
            {
                if (_records.Remove(facility + "\x01" + name))
                    _dirty = true;
            }
        }

        public bool Exists(string facility, string name)
        {
            lock (_lock)
            {
                return _records.ContainsKey(facility + "\x01" + name);
            }
        }

        public string Get(string facility, string name)
        {
            lock (_lock)
            {
                return _records.TryGetValue(facility + "\x01" + name, out var obj)
                    ? obj.Value
                    : null;
            }
        }

        public void Purge(string facility, int ageSeconds)
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                var remove = _records.Values
                    .Where(x => x.Facility.Like(facility) && (now - x.Updated).TotalSeconds > ageSeconds).ToList();
                foreach (var obj in remove)
                    Delete(obj.Facility, obj.Name);
            }
        }

        public void Set(string facility, string name, string value)
        {
            lock (_lock)
            {
                var obj = new FileObject
                {
                    Facility = facility,
                    Name = name,
                    Value = value,
                    Updated = DateTime.Now
                };

                _records[facility + "\x01" + name] = obj;
                _dirty = true;
            }
        }
    }
}

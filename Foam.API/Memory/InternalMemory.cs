using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons;

namespace Foam.API.Memory
{
    /// <summary>
    /// Internal volatile memory - use only for debugging.
    /// </summary>
    public class InternalMemory : IMemory
    {
        private readonly object _lock = new object();

        private class FileObject
        {
            public string Facility { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public DateTime Updated { get; set; }
        }

        private readonly Dictionary<string, FileObject> _records = new Dictionary<string, FileObject>(StringComparer.CurrentCultureIgnoreCase);

        public void Dispose()
        {
        }

        public void Delete(string facility, string name)
        {
            lock (_lock)
            {
                _records.Remove(facility + "\x01" + name);
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
            }
        }
    }
}

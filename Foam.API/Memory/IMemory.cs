using System;

namespace Foam.API.Memory
{
    public interface IMemory : IDisposable
    {
        void Delete(string facility, string name);
        bool Exists(string facility, string name);
        string Get(string facility, string name);
        void Purge(string facility, int ageSeconds);
        void Set(string facility, string name, string value);
    }
}

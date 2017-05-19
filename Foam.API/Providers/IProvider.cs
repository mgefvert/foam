using System;
using System.Collections.Generic;
using Foam.API.Configuration;
using Foam.API.Files;
using Foam.API.Transactions;

namespace Foam.API.Providers
{
    public interface IProvider : IDisposable
    {
        bool Readonly { get; }

        bool CanHandleUri(Uri source);
        bool CanHandleProtocol(string protocol);
        void Delete(string filespec);
        FileList Fetch(Uri location, string mask, CommitBuffer commitBuffer);
        void Write(Uri location, List<FileItem> files, CommitBuffer commitBuffer, OverwriteMode overwrite);
    }
}
